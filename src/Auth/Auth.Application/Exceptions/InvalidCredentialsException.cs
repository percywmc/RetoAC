namespace Auth.Application.Exceptions;

public class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException() : base("Usuario o contraseña inválidos.")
    {
    }
}
