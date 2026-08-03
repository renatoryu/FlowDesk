using FlowDesk.Domain.Entities;
using FluentValidation;

namespace FlowDesk.Application.Authentication.Login;

public sealed class LoginUserCommandValidator
    : AbstractValidator<LoginUserCommand>
{
    private const int MaximumPasswordLength = 128;

    public LoginUserCommandValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
            .MaximumLength(User.MaxEmailLength)
            .EmailAddress();

        RuleFor(command => command.Password)
            .NotEmpty()
            .MaximumLength(MaximumPasswordLength);
    }
}
