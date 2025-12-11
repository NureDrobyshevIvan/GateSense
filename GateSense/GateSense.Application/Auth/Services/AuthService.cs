using GateSense.Application.Auth.Interfaces;
using Domain.Models;
using Domain.Models.Auth;
using Domain.Models.DTOS.Auth.Models;
using Domain.Models.DTOS.Auth.Responses;
using Infrastructure.Common.Cookies;
using Infrastructure.Common.Errors.Auth;
using Infrastructure.Common.Errors.Common.Repository;
using Infrastructure.Common.JWT;
using Infrastructure.Common.ResultPattern;
using Infrastructure.Data.UnitOfWork;
using Infrastructure.Repository.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GateSense.Application.Auth.Services;

public class AuthService : IAuthService
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    private readonly UserManager<ApplicationUser> _userManager;

    private readonly ITokenService _tokenService;

    private readonly IRoleService _roleService;



    private readonly IUnitOfWork _unitOfWork;

    private readonly IGenericRepository<RefreshToken> _refreshTokenRepository;

    private readonly ICookieService _cookieService;

    public AuthService(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IRoleService roleService,
        IUnitOfWork unitOfWork,
        IGenericRepository<RefreshToken> refreshTokenRepository,
        ICookieService cookieService
    )
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _tokenService = tokenService;
        _roleService = roleService;
        _unitOfWork = unitOfWork;
        _refreshTokenRepository = refreshTokenRepository;
        _cookieService = cookieService;
    }

    public async Task<Result<int>> GetIdByEmail(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return Result<int>.Failure(UserErrors.UserNotFoundError());

        return Result<int>.Success(user.Id);
    }

    public async Task<Result<IEnumerable<string>>> GetRolesByEmail(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user != null)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return Result<IEnumerable<string>>.Success(roles);
        }

        return Result<IEnumerable<string>>.Failure(UserErrors.UserNotFoundError());
    }

    public async Task<Result<LoginResponse>> GetUserProfile(string? email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return Result<LoginResponse>.Failure(UserErrors.UserNotFoundError());
        }
        var roles = await _userManager.GetRolesAsync(user);

        var role = UserRoles.User;
        if (roles.Contains(UserRoles.Admin))
        {
            role = UserRoles.Admin;
        }

        return Result<LoginResponse>.Success(new LoginResponse
        {
            UserName = user.UserName,
            Email = user.Email,
            Role = role
        });
    }

    public async Task<Result<LoginResponse>> RefreshToken()
    {
        var refreshToken = _cookieService.GetRefreshToken();

        var includes = new List<Func<IQueryable<RefreshToken>, IQueryable<RefreshToken>>>
        {
            q => q.Include(r => r.User),
        };

        var refreshTokenResult = await _refreshTokenRepository
            .GetSingleByConditionAsync(s => s.Token == refreshToken, includes);

        if (!refreshTokenResult.IsSuccess || refreshTokenResult.Value.ExpiresOnUtc < DateTime.UtcNow)
        {
            //logout the user
            await _signInManager.SignOutAsync();
            _cookieService.ClearAuthCookies();

            return Result<LoginResponse>.Failure(UserErrors.InvalidOrExpiredRefreshToken());
        }

        var refreshTokenDto = _tokenService.GenerateRefreshToken();

        var newRefreshToken = new RefreshToken()
        {
            Token = refreshTokenDto.bytes,
            User = refreshTokenResult.Value.User,
            ExpiresOnUtc = DateTime.UtcNow.AddDays(refreshTokenDto.ExpirationTimeInDays),
        };
        await _unitOfWork.BeginTransactionAsync();

        //clear existing refresh tokens and issue a new refresh token
        var deleteRefreshTokenResult = await _refreshTokenRepository
            .DeleteAsync(s => s.UserId == refreshTokenResult.Value.User.Id);
        if (!deleteRefreshTokenResult.IsSuccess)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return Result<LoginResponse>.Failure(RepositoryErrors<RefreshToken>.DeleteError);
        }

        var addNewRefreshTokenResult = await _refreshTokenRepository.AddAsync(newRefreshToken);
        if (!addNewRefreshTokenResult.IsSuccess)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return Result<LoginResponse>.Failure(RepositoryErrors<RefreshToken>.AddError);
        }

        await _unitOfWork.CommitTransactionAsync();

        var accessToken = _tokenService.GenerateAuthToken(refreshTokenResult.Value.User);

        //This will attach the HTTP only cookies to the response
        _cookieService.SetAuthCookies(accessToken, newRefreshToken.Token);

        var roles = await _userManager.GetRolesAsync(refreshTokenResult.Value.User);
        var role = UserRoles.User;
        if (roles.Contains(UserRoles.Admin))
        {
            role = UserRoles.Admin;
        }

        var response = new LoginResponse
        {
            UserName = refreshTokenResult.Value.User.UserName,
            Email = refreshTokenResult.Value.User.Email,
            Role = role,
            AccessToken = accessToken,
            RefreshToken = newRefreshToken.Token
        };

        return Result<LoginResponse>.Success(response);
    }

    public async Task<Result> RegisterAsync(RegisterModel registerModel)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var appUser = new ApplicationUser
            {
                UserName = registerModel.UserName,
                Email = registerModel.Email,
                FirstName = registerModel.FirstName,
                LastName = registerModel.LastName,
            };

            var userResult = await _userManager.CreateAsync(appUser, registerModel.Password);

            if (!userResult.Succeeded)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result.Failure(UserErrors.UserNotCreatedError(userResult.Errors.First().Description));
            }

            var resRolesResult = await _roleService.AddToRolesAsync(appUser, UserRoles.User);


            if (resRolesResult == false)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result.Failure(UserErrors.UserNotAssignedToRole());
            }

            await _unitOfWork.CommitTransactionAsync();
            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return Result.Failure(RepositoryErrorMapper<ApplicationUser>.Map(ex));
        }
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginModel loginModel)
    {
        var user = await _userManager.FindByEmailAsync(loginModel.Login) ??
                   await _userManager.FindByNameAsync(loginModel.Login);

        if (user is null)
        {
            return Result<LoginResponse>.Failure(UserErrors.UserNotFoundError());
        }

        if (!await _userManager.IsEmailConfirmedAsync(user))
        {
            return Result<LoginResponse>.Failure(UserErrors.UserEmailNotConfirmed());
        }

        var result = await _userManager.CheckPasswordAsync(user, loginModel.Password);
        if (result)
        {
            //clear existing refresh tokens and issue a new refresh token
            await _unitOfWork.BeginTransactionAsync();

            var exists = await _refreshTokenRepository.GetSingleByConditionAsync(s => s.UserId == user.Id);

            if (exists.IsSuccess)
            {
                var deleteRefreshTokenResult = await _refreshTokenRepository
                    .DeleteAsync(s => s.UserId == user.Id);

                if (!deleteRefreshTokenResult.IsSuccess)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result<LoginResponse>.Failure(RepositoryErrors<RefreshToken>.DeleteError);
                }
            }

            var refreshTokenDto = _tokenService.GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                User = user,
                Token = refreshTokenDto.bytes,
                ExpiresOnUtc = DateTime.UtcNow.AddDays(refreshTokenDto.ExpirationTimeInDays),
            };

            var addRefreshTokenResult = await _refreshTokenRepository.AddAsync(refreshTokenEntity);

            if (!addRefreshTokenResult.IsSuccess)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result<LoginResponse>.Failure(RepositoryErrors<RefreshToken>.AddError);
            }

            await _unitOfWork.CommitTransactionAsync();

            var accessToken = _tokenService.GenerateAuthToken(user);

            //This will attach the HTTP only cookies to the response
            _cookieService.SetAuthCookies(accessToken, refreshTokenEntity.Token);

            var roles = await _userManager.GetRolesAsync(user);

            var role = UserRoles.User;
            if (roles.Contains(UserRoles.Admin))
            {
                role = UserRoles.Admin;
            }

            return Result<LoginResponse>.Success(new LoginResponse
            {
                UserName = user.UserName,
                Email = user.Email,
                Role = role,
                AccessToken = accessToken,
                RefreshToken = refreshTokenEntity.Token
            });
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            return Result<LoginResponse>.Failure(UserErrors.UserLockedOut());
        }

        return Result<LoginResponse>.Failure(UserErrors.UserInvalidCredentials());
    }

    public async Task<Result> LogoutAsync()
    {
        await _signInManager.SignOutAsync();

        _cookieService.ClearAuthCookies();

        return Result.Success();
    }
}