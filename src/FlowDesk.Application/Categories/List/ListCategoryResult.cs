namespace FlowDesk.Application.Categories.List;

public sealed record ListCategoryResult(
    Guid Id,
    string Name,
    string? Description);
