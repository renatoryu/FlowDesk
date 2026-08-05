using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Domain.Entities;

namespace FlowDesk.Application.Companies.Deactivate;

public sealed class DeactivateCompanyHandler
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateCompanyHandler(
        ICompanyRepository companyRepository,
        IUnitOfWork unitOfWork)
    {
        _companyRepository = companyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(
        DeactivateCompanyCommand command,
        CancellationToken cancellationToken = default)
    {
        Company company =
            await _companyRepository.GetByIdAsync(
                command.Id,
                cancellationToken)
            ?? throw new NotFoundException(
                "Company was not found.");

        if (!company.IsActive)
        {
            return;
        }

        company.Deactivate();

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}
