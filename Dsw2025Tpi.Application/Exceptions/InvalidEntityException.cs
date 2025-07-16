namespace Dsw2025Tpi.Application.Exceptions;

public class InvalidEntityException : ApplicationException
{
    public InvalidEntityException(string message) : base(message) { }
}
