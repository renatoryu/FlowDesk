using FluentValidation;

namespace FlowDesk.Application.Tickets.List;

public sealed class ListTicketsQueryValidator
    : AbstractValidator<ListTicketsQuery>
{
    public ListTicketsQueryValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(query => query.CategoryId)
            .Must(categoryId =>
                !categoryId.HasValue ||
                categoryId.Value != Guid.Empty)
            .WithMessage("Category id must not be empty.");

        RuleFor(query => query.Priority)
            .Must(priority =>
                !priority.HasValue ||
                Enum.IsDefined(priority.Value))
            .WithMessage("Priority must be valid.");

        RuleFor(query => query.Status)
            .Must(status =>
                !status.HasValue ||
                Enum.IsDefined(status.Value))
            .WithMessage("Status must be valid.");
    }
}
