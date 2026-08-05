namespace FlowDesk.Api.Contracts.Companies;

public sealed record UpdateCompanyRequest(
    string Name,
    string ContactEmail);
