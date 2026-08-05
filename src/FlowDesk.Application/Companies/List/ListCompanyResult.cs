namespace FlowDesk.Application.Companies.List;

public sealed record ListCompanyResult(
    Guid Id,
    string Name,
    string TaxId,
    string ContactEmail,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
