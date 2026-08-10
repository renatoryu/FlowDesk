namespace FlowDesk.Application.Users.AssignCompany;

public sealed record AssignUserCompanyCommand(
    Guid UserId,
    Guid CompanyId);
