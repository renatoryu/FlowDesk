using FlowDesk.Domain.Entities;
using FluentValidation;

namespace FlowDesk.Application.Tickets.Update;

public sealed class UpdateTicketCommandValidator
    : AbstractValidator<UpdateTicketCommand>
{
    public UpdateTicketCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();

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
