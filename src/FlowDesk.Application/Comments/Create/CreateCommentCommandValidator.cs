using FlowDesk.Domain.Entities;
using FluentValidation;

namespace FlowDesk.Application.Comments.Create;

public sealed class CreateCommentCommandValidator
    : AbstractValidator<CreateCommentCommand>
{
    public CreateCommentCommandValidator()
    {
        RuleFor(command => command.TicketId)
            .NotEmpty();

        RuleFor(command => command.Content)
            .Must(content => !string.IsNullOrWhiteSpace(content))
            .WithMessage("'Content' must be provided.")
            .MaximumLength(Comment.MaxContentLength);
    }
}
