using FluentValidation;

namespace FlowDesk.Application.Tickets.GetById;

public sealed class GetTicketByIdQueryValidator
    : AbstractValidator<GetTicketByIdQuery>
{
    public GetTicketByIdQueryValidator()
    {
        RuleFor(query => query.Id)
            .NotEmpty();
    }
}
