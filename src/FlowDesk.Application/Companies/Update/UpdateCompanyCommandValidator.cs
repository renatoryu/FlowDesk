using FlowDesk.Domain.Entities;
using FluentValidation;

namespace FlowDesk.Application.Companies.Update;

public sealed class UpdateCompanyCommandValidator
    : AbstractValidator<UpdateCompanyCommand>
{
    public UpdateCompanyCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();

        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(Company.MaxNameLength);

        RuleFor(command => command.ContactEmail)
            .NotEmpty()
            .MaximumLength(Company.MaxContactEmailLength)
            .Must(contactEmail =>
                Company.IsValidContactEmail(contactEmail))
            .WithMessage("Contact email must be valid.");
    }
}
