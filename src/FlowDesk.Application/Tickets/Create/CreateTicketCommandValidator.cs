using FlowDesk.Domain.Entities;
using FluentValidation;

namespace FlowDesk.Application.Tickets.Create;

public sealed class CreateTicketCommandValidator
    : AbstractValidator<CreateTicketCommand>
{
    public CreateTicketCommandValidator()
    {
        RuleFor(command => command.CategoryId)
            .NotEmpty();

        RuleFor(command => command.Title)
            .NotEmpty()
            .MaximumLength(Ticket.MaxTitleLength);

        RuleFor(command => command.Description)
            .NotEmpty()
            .MaximumLength(Ticket.MaxDescriptionLength);

        RuleFor(command => command.Priority)
            .IsInEnum();
    }
}
