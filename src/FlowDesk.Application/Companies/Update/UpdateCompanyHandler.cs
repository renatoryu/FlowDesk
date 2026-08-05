using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Domain.Entities;
using FluentValidation;

namespace FlowDesk.Application.Companies.Update;

public sealed class UpdateCompanyHandler
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<UpdateCompanyCommand> _validator;

    public UpdateCompanyHandler(
        ICompanyRepository companyRepository,
        IUnitOfWork unitOfWork,
        IValidator<UpdateCompanyCommand> validator)
    {
        _companyRepository = companyRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<UpdateCompanyResult> HandleAsync(
        UpdateCompanyCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(
            command,
            cancellationToken);

        Company company =
            await _companyRepository.GetByIdAsync(
                command.Id,
                cancellationToken)
            ?? throw new NotFoundException(
                "Company was not found.");

        company.UpdateDetails(
            command.Name,
            command.ContactEmail);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return new UpdateCompanyResult(
            company.Id,
            company.Name,
            company.TaxId,
            company.ContactEmail,
            company.IsActive,
            company.CreatedAtUtc,
            company.UpdatedAtUtc);
    }
}
