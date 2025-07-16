namespace Dsw2025Tpi.Application.Exceptions;

public class InsufficientStockException : ApplicationException
{
    public InsufficientStockException(string message) : base(message) { }
}
