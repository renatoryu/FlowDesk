namespace FlowDesk.Application.Companies.Create;

public sealed record CreateCompanyCommand(
    string Name,
    string TaxId,
    string ContactEmail);
