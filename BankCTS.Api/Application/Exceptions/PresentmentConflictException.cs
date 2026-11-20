
namespace BankCTS.Api.Application.Exceptions;

public class PresentmentConflictException : Exception
{
    public PresentmentConflictException(string message)
        : base(message)
    {
    }
}
