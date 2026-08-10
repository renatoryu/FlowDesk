using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FluentValidation;

namespace FlowDesk.Application.Users.AssignCompany;

public sealed class AssignUserCompanyHandler
{
    private readonly IUserRepository _userRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<AssignUserCompanyCommand> _validator;

    public AssignUserCompanyHandler(
        IUserRepository userRepository,
        ICompanyRepository companyRepository,
        IUnitOfWork unitOfWork,
        IValidator<AssignUserCompanyCommand> validator)
    {
        _userRepository = userRepository;
        _companyRepository = companyRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<AssignUserCompanyResult> HandleAsync(
        AssignUserCompanyCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(
            command,
            cancellationToken);

        User user =
            await _userRepository.GetForUpdateAsync(
                command.UserId,
                cancellationToken)
            ?? throw new NotFoundException(
                "User was not found.");

        if (!user.IsActive)
        {
            throw new ConflictException(
                "Inactive users cannot be assigned to a company.");
        }

        if (user.Role != UserRole.Customer)
        {
            throw new ConflictException(
                "Only customers can be assigned to a company.");
        }

        Company company =
            await _companyRepository.GetByIdAsync(
                command.CompanyId,
                cancellationToken)
            ?? throw new NotFoundException(
                "Company was not found.");

        if (!company.IsActive)
        {
            throw new ConflictException(
                "Inactive companies cannot receive customers.");
        }

        if (user.CompanyId == company.Id)
        {
            return new AssignUserCompanyResult(
                user.Id,
                company.Id);
        }

        user.AssignToCompany(company.Id);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return new AssignUserCompanyResult(
            user.Id,
            company.Id);
    }
}
