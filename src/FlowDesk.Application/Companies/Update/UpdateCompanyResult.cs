namespace FlowDesk.Application.Companies.Update;

public sealed record UpdateCompanyResult(
    Guid Id,
    string Name,
    string TaxId,
    string ContactEmail,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
