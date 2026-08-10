using FluentValidation;

namespace FlowDesk.Application.Tickets.Delete;

public sealed class DeleteTicketCommandValidator
    : AbstractValidator<DeleteTicketCommand>
{
    public DeleteTicketCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();
    }
}
