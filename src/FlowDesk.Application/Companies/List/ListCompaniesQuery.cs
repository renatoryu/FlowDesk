namespace FlowDesk.Application.Companies.List;

public sealed record ListCompaniesQuery(
    bool IncludeInactive = false);
