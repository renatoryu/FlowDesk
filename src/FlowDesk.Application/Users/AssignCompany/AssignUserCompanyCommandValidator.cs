using FluentValidation;

namespace FlowDesk.Application.Users.AssignCompany;

public sealed class AssignUserCompanyCommandValidator
    : AbstractValidator<AssignUserCompanyCommand>
{
    public AssignUserCompanyCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty();

        RuleFor(command => command.CompanyId)
            .NotEmpty();
    }
}
