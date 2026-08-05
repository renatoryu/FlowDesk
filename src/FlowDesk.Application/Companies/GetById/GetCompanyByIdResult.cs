namespace FlowDesk.Application.Companies.GetById;

public sealed record GetCompanyByIdResult(
    Guid Id,
    string Name,
    string TaxId,
    string ContactEmail,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
