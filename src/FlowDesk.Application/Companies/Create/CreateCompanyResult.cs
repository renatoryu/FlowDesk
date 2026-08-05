namespace FlowDesk.Application.Companies.Create;

public sealed record CreateCompanyResult(
    Guid Id,
    string Name,
    string TaxId,
    string ContactEmail,
    bool IsActive,
    DateTime CreatedAtUtc);
