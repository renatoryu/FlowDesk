using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Domain.Entities;
using FluentValidation;

namespace FlowDesk.Application.Companies.Create;

public sealed class CreateCompanyHandler
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateCompanyCommand> _validator;

    public CreateCompanyHandler(
        ICompanyRepository companyRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateCompanyCommand> validator)
    {
        _companyRepository = companyRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<CreateCompanyResult> HandleAsync(
        CreateCompanyCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(
            command,
            cancellationToken);

        var company = new Company(
            command.Name,
            command.TaxId,
            command.ContactEmail);

        bool taxIdAlreadyExists =
            await _companyRepository.ExistsByTaxIdAsync(
                company.TaxId,
                cancellationToken);

        if (taxIdAlreadyExists)
        {
            throw new ConflictException(
                "A company with this tax id is already registered.");
        }

        await _companyRepository.AddAsync(
            company,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return new CreateCompanyResult(
            company.Id,
            company.Name,
            company.TaxId,
            company.ContactEmail,
            company.IsActive,
            company.CreatedAtUtc);
    }
}
