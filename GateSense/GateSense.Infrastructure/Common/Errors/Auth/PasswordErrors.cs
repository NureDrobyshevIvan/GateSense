namespace Infrastructure.Common.Errors.Auth;

public class PasswordErrors
{
    public static Error PasswordNotChangedError()
    {
        return Error.Validation("auth.USER_PASSWORD_NOT_CHANGED","The password wasn't changed");
    }
}