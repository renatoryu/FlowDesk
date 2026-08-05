using FlowDesk.Domain.Entities;
using FluentValidation;

namespace FlowDesk.Application.Companies.Create;

public sealed class CreateCompanyCommandValidator
    : AbstractValidator<CreateCompanyCommand>
{
    public CreateCompanyCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(Company.MaxNameLength);

        RuleFor(command => command.TaxId)
            .NotEmpty()
            .MaximumLength(Company.MaxTaxIdInputLength)
            .Must(taxId => Company.IsValidTaxId(taxId))
            .WithMessage("Tax id must be a valid CNPJ.");

        RuleFor(command => command.ContactEmail)
            .NotEmpty()
            .MaximumLength(Company.MaxContactEmailLength)
            .Must(contactEmail =>
                Company.IsValidContactEmail(contactEmail))
                .WithMessage("Contact email must be valid.");
    }
}
