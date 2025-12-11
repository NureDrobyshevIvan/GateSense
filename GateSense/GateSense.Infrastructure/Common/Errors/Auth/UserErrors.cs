namespace Infrastructure.Common.Errors.Auth;

public static class UserErrors
{
    public static Error UserNotFoundError()
    {
        return Error.NotFound("auth.USER_NOT_FOUND", "User not found");
    } 

    public static Error UserNotCreatedError(string description)
    {
        return Error.Validation("auth.USER_VALIDATION", description);
    }

    public static Error UserNotAssignedToRole()
    {
        return Error.InternalServerError("auth.USER_ROLES_NOT_ASSIGNED", "User couldn't be assigned to roles");  
    } 
    
    public static Error UserEmailNotConfirmed()
    {
        return Error.Validation("auth.USER_EMAIL_CONFIRMATION", "User email not confirmed");  
    }
    
    public static Error UserLockedOut()
    {
        return Error.Validation("auth.USER_ACCOUNT_LOCKED", "User account is locked out");  
    }
    
    public static Error UserInvalidCredentials()
    {
        return Error.Validation("auth.USER_CREDENTIALS_INVALID", "Wrong credentials, try again");  
    }
    
    public static Error InvalidOrExpiredRefreshToken()
    {
        return Error.Validation("auth.USER_TOKEN_INVALID", "Refresh token that you provided is invalid");  
    }

    public static Error UserNameIsNotUnique()
    {
        return Error.Conflict("auth.USER_USERNAME_NOT_UNIQUE", "User name is not unique");
    }
    
    public static Error EmailIsNotUnique()
    {
        return Error.Conflict("auth.USER_EMAIL_NOT_UNIQUE", "Email is not unique");
    }
}