using FluentValidation;

namespace FlowDesk.Application.Attachments.List;

public sealed class ListTicketAttachmentsQueryValidator
    : AbstractValidator<ListTicketAttachmentsQuery>
{
    public ListTicketAttachmentsQueryValidator()
    {
        RuleFor(query => query.TicketId)
            .NotEmpty();
    }
}
