namespace FlowDesk.Application.Users.AssignCompany;

public sealed record AssignUserCompanyResult(
    Guid UserId,
    Guid CompanyId);
