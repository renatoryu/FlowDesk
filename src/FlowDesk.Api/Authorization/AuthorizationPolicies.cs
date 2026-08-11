namespace FlowDesk.Api.Authorization;

public static class AuthorizationPolicies
{
    public const string CompanyRead = "CompanyRead";
    public const string CompanyWrite = "CompanyWrite";
    public const string UserCompanyWrite = "UserCompanyWrite";
    public const string TicketCreate = "TicketCreate";
    public const string TicketRead = "TicketRead";
    public const string TicketUpdate = "TicketUpdate";
    public const string TicketStatusChange = "TicketStatusChange";
    public const string TicketDelete = "TicketDelete";
    public const string CommentCreate = "CommentCreate";
    public const string CommentRead = "CommentRead";
    public const string DashboardRead = "DashboardRead";
    public const string AttachmentUpload = "AttachmentUpload";
    public const string AttachmentRead = "AttachmentRead";
}
