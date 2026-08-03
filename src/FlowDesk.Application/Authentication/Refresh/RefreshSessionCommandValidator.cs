using FluentValidation;

namespace FlowDesk.Application.Authentication.Refresh;

public sealed class RefreshSessionCommandValidator
    : AbstractValidator<RefreshSessionCommand>
{
    private const int MaximumTokenLength = 512;

    public RefreshSessionCommandValidator()
    {
        RuleFor(command => command.RefreshToken)
            .NotEmpty()
            .MaximumLength(MaximumTokenLength);
    }
}
