using FlowDesk.Domain.Entities;
using FluentValidation;

namespace FlowDesk.Application.Authentication.Register;

public sealed class RegisterUserCommandValidator
    : AbstractValidator<RegisterUserCommand>
{
    private const int MinimumPasswordLength = 8;
    private const int MaximumPasswordLength = 128;

    public RegisterUserCommandValidator()
    {
        RuleFor(command => command.FullName)
            .NotEmpty()
            .MaximumLength(User.MaxFullNameLength);

        RuleFor(command => command.Email)
            .NotEmpty()
            .MaximumLength(User.MaxEmailLength)
            .EmailAddress();

        RuleFor(command => command.Password)
            .NotEmpty()
            .MinimumLength(MinimumPasswordLength)
            .MaximumLength(MaximumPasswordLength)
            .Matches("[A-Z]")
            .WithMessage("Password must contain an uppercase letter.")
            .Matches("[a-z]")
            .WithMessage("Password must contain a lowercase letter.")
            .Matches("[0-9]")
            .WithMessage("Password must contain a number.")
            .Matches("[^a-zA-Z0-9]")
            .WithMessage("Password must contain a special character.");
    }
}
