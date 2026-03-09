namespace Predictly.Domain.Exceptions;

public sealed class BusinessRuleException : DomainException
{
    public required string Code { get; init; }

    public BusinessRuleException(string code, string message) : base(message)
    {
        Code = code;
    }
}
