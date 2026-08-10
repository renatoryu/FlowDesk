namespace FlowDesk.Domain.Common;

public sealed class DomainRuleException : Exception
{
    public DomainRuleException()
    {
    }

    public DomainRuleException(string message)
        : base(message)
    {
    }

    public DomainRuleException(
        string message,
        Exception innerException)
        : base(message, innerException)
    {
    }
}
