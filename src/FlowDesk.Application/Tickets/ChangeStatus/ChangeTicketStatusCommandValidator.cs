using FluentValidation;

namespace FlowDesk.Application.Tickets.ChangeStatus;

public sealed class ChangeTicketStatusCommandValidator
    : AbstractValidator<ChangeTicketStatusCommand>
{
    public ChangeTicketStatusCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();

        RuleFor(command => command.Status)
            .IsInEnum();
    }
}
