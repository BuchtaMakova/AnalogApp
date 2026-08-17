namespace AnalogHub.Application.Common.Exceptions;

public sealed class AuthenticationFailedException : Exception
{
    public AuthenticationFailedException()
        : base("Invalid email or password.")
    {
    }
}
