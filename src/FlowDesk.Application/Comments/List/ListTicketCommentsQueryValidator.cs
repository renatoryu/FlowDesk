using FluentValidation;

namespace FlowDesk.Application.Comments.List;

public sealed class ListTicketCommentsQueryValidator
    : AbstractValidator<ListTicketCommentsQuery>
{
    public ListTicketCommentsQueryValidator()
    {
        RuleFor(query => query.TicketId)
            .NotEmpty();
    }
}
