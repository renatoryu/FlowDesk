namespace FlowDesk.Application.Companies.Update;

public sealed record UpdateCompanyCommand(
    Guid Id,
    string Name,
    string ContactEmail);
