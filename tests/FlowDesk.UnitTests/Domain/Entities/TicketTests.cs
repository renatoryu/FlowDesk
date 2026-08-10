using FlowDesk.Domain.Common;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;

namespace FlowDesk.UnitTests.Domain.Entities;

public sealed class TicketTests
{
    [Fact]
    public void ConstructorWithValidDataCreatesOpenTicket()
    {
        Guid companyId = Guid.NewGuid();
        Guid categoryId = Guid.NewGuid();
        Guid requesterId = Guid.NewGuid();

        DateTime beforeCreation = DateTime.UtcNow;

        var ticket = new Ticket(
            companyId,
            categoryId,
            requesterId,
            "  Cannot access the system  ",
            "  The application rejects my credentials.  ",
            TicketPriority.High);

        DateTime afterCreation = DateTime.UtcNow;

        Assert.NotEqual(Guid.Empty, ticket.Id);
        Assert.Equal(companyId, ticket.CompanyId);
        Assert.Equal(categoryId, ticket.CategoryId);
        Assert.Equal(requesterId, ticket.RequesterId);
        Assert.Equal(
            "Cannot access the system",
            ticket.Title);
        Assert.Equal(
            "The application rejects my credentials.",
            ticket.Description);
        Assert.Equal(
            TicketPriority.High,
            ticket.Priority);
        Assert.Equal(TicketStatus.Open, ticket.Status);
        Assert.Equal(
            ticket.CreatedAtUtc,
            ticket.StatusChangedAtUtc);
        Assert.Null(ticket.ResolvedAtUtc);
        Assert.Null(ticket.ClosedAtUtc);
        Assert.False(ticket.IsDeleted);
        Assert.Null(ticket.DeletedAtUtc);
        Assert.Null(ticket.DeletedByUserId);
        Assert.InRange(
            ticket.CreatedAtUtc,
            beforeCreation,
            afterCreation);
    }

    [Theory]
    [InlineData("companyId")]
    [InlineData("categoryId")]
    [InlineData("requesterId")]
    public void ConstructorWithEmptyRequiredIdThrowsArgumentException(
        string parameterName)
    {
        Guid companyId = Guid.NewGuid();
        Guid categoryId = Guid.NewGuid();
        Guid requesterId = Guid.NewGuid();

        switch (parameterName)
        {
            case "companyId":
                companyId = Guid.Empty;
                break;

            case "categoryId":
                categoryId = Guid.Empty;
                break;

            case "requesterId":
                requesterId = Guid.Empty;
                break;
        }

        ArgumentException exception =
            Assert.Throws<ArgumentException>(
                () => new Ticket(
                    companyId,
                    categoryId,
                    requesterId,
                    "Title",
                    "Description",
                    TicketPriority.Medium));

        Assert.Equal(
            parameterName,
            exception.ParamName);
    }

    [Fact]
    public void ConstructorWithBlankTitleThrowsArgumentException()
    {
        ArgumentException exception =
            Assert.Throws<ArgumentException>(
                () => CreateTicket(title: " "));

        Assert.Equal("title", exception.ParamName);
    }

    [Fact]
    public void ConstructorWithTitleExceedingLimitThrowsArgumentException()
    {
        string longTitle =
            new('A', Ticket.MaxTitleLength + 1);

        ArgumentException exception =
            Assert.Throws<ArgumentException>(
                () => CreateTicket(title: longTitle));

        Assert.Equal("title", exception.ParamName);
    }

    [Fact]
    public void ConstructorWithBlankDescriptionThrowsArgumentException()
    {
        ArgumentException exception =
            Assert.Throws<ArgumentException>(
                () => CreateTicket(description: " "));

        Assert.Equal(
            "description",
            exception.ParamName);
    }

    [Fact]
    public void ConstructorWithDescriptionExceedingLimitThrowsArgumentException()
    {
        string longDescription =
            new('A', Ticket.MaxDescriptionLength + 1);

        ArgumentException exception =
            Assert.Throws<ArgumentException>(
                () => CreateTicket(
                    description: longDescription));

        Assert.Equal(
            "description",
            exception.ParamName);
    }

    [Fact]
    public void ConstructorWithUndefinedPriorityThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateTicket(
                priority: (TicketPriority)255));
    }

    [Fact]
    public void UpdateDetailsWithValidDataUpdatesTicket()
    {
        Ticket ticket = CreateTicket();
        Guid newCategoryId = Guid.NewGuid();

        ticket.UpdateDetails(
            "  Updated title  ",
            "  Updated description.  ",
            newCategoryId,
            TicketPriority.Critical);

        Assert.Equal("Updated title", ticket.Title);
        Assert.Equal(
            "Updated description.",
            ticket.Description);
        Assert.Equal(
            newCategoryId,
            ticket.CategoryId);
        Assert.Equal(
            TicketPriority.Critical,
            ticket.Priority);
    }

    [Fact]
    public void UpdateDetailsWithInvalidDataDoesNotChangeTicket()
    {
        Ticket ticket = CreateTicket();

        string originalTitle = ticket.Title;
        string originalDescription =
            ticket.Description;
        Guid originalCategoryId =
            ticket.CategoryId;
        TicketPriority originalPriority =
            ticket.Priority;
        DateTime originalUpdatedAt =
            ticket.UpdatedAtUtc;

        string longDescription =
            new('A', Ticket.MaxDescriptionLength + 1);

        Assert.Throws<ArgumentException>(
            () => ticket.UpdateDetails(
                "Changed title",
                longDescription,
                Guid.NewGuid(),
                TicketPriority.High));

        Assert.Equal(originalTitle, ticket.Title);
        Assert.Equal(
            originalDescription,
            ticket.Description);
        Assert.Equal(
            originalCategoryId,
            ticket.CategoryId);
        Assert.Equal(
            originalPriority,
            ticket.Priority);
        Assert.Equal(
            originalUpdatedAt,
            ticket.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetailsWithSameDataIsIdempotent()
    {
        Ticket ticket = CreateTicket();

        DateTime originalUpdatedAt =
            ticket.UpdatedAtUtc;

        ticket.UpdateDetails(
            ticket.Title,
            ticket.Description,
            ticket.CategoryId,
            ticket.Priority);

        Assert.Equal(
            originalUpdatedAt,
            ticket.UpdatedAtUtc);
    }

    [Fact]
    public void ChangeStatusThroughLifecycleSetsTimestamps()
    {
        Ticket ticket = CreateTicket();

        ticket.ChangeStatus(
            TicketStatus.InProgress);

        Assert.Equal(
            TicketStatus.InProgress,
            ticket.Status);
        Assert.Null(ticket.ResolvedAtUtc);
        Assert.Null(ticket.ClosedAtUtc);

        ticket.ChangeStatus(
            TicketStatus.Resolved);

        DateTime? resolvedAt =
            ticket.ResolvedAtUtc;

        Assert.Equal(
            TicketStatus.Resolved,
            ticket.Status);
        Assert.NotNull(resolvedAt);
        Assert.Null(ticket.ClosedAtUtc);

        ticket.ChangeStatus(
            TicketStatus.Closed);

        Assert.Equal(
            TicketStatus.Closed,
            ticket.Status);
        Assert.Equal(
            resolvedAt,
            ticket.ResolvedAtUtc);
        Assert.NotNull(ticket.ClosedAtUtc);
    }

    [Fact]
    public void ChangeStatusFromResolvedToInProgressClearsResolution()
    {
        Ticket ticket = CreateTicket();

        ticket.ChangeStatus(
            TicketStatus.InProgress);

        ticket.ChangeStatus(
            TicketStatus.Resolved);

        ticket.ChangeStatus(
            TicketStatus.InProgress);

        Assert.Equal(
            TicketStatus.InProgress,
            ticket.Status);
        Assert.Null(ticket.ResolvedAtUtc);
        Assert.Null(ticket.ClosedAtUtc);
    }

    [Fact]
    public void ChangeStatusWithSameStatusIsIdempotent()
    {
        Ticket ticket = CreateTicket();

        DateTime originalStatusChangedAt =
            ticket.StatusChangedAtUtc;
        DateTime originalUpdatedAt =
            ticket.UpdatedAtUtc;

        ticket.ChangeStatus(TicketStatus.Open);

        Assert.Equal(
            originalStatusChangedAt,
            ticket.StatusChangedAtUtc);
        Assert.Equal(
            originalUpdatedAt,
            ticket.UpdatedAtUtc);
    }

    [Fact]
    public void ChangeStatusWithInvalidTransitionThrowsDomainRuleException()
    {
        Ticket ticket = CreateTicket();

        Assert.Throws<DomainRuleException>(
            () => ticket.ChangeStatus(
                TicketStatus.Resolved));

        Assert.Equal(
            TicketStatus.Open,
            ticket.Status);
        Assert.Null(ticket.ResolvedAtUtc);
    }

    [Fact]
    public void ChangeStatusWithUndefinedStatusThrowsArgumentOutOfRangeException()
    {
        Ticket ticket = CreateTicket();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => ticket.ChangeStatus(
                (TicketStatus)255));

        Assert.Equal(
            TicketStatus.Open,
            ticket.Status);
    }

    [Fact]
    public void UpdateDetailsOnClosedTicketThrowsDomainRuleException()
    {
        Ticket ticket = CreateClosedTicket();

        Assert.Throws<DomainRuleException>(
            () => ticket.UpdateDetails(
                "Changed title",
                "Changed description",
                Guid.NewGuid(),
                TicketPriority.Low));
    }

    [Fact]
    public void DeleteWithValidUserPerformsLogicalDeletion()
    {
        Ticket ticket = CreateTicket();
        Guid deletedByUserId = Guid.NewGuid();

        DateTime beforeDelete = DateTime.UtcNow;

        ticket.Delete(deletedByUserId);

        DateTime afterDelete = DateTime.UtcNow;

        Assert.True(ticket.IsDeleted);
        Assert.Equal(
            deletedByUserId,
            ticket.DeletedByUserId);
        Assert.NotNull(ticket.DeletedAtUtc);
        Assert.InRange(
            ticket.DeletedAtUtc.Value,
            beforeDelete,
            afterDelete);
    }

    [Fact]
    public void DeleteWithEmptyUserIdThrowsArgumentException()
    {
        Ticket ticket = CreateTicket();

        ArgumentException exception =
            Assert.Throws<ArgumentException>(
                () => ticket.Delete(Guid.Empty));

        Assert.Equal(
            "deletedByUserId",
            exception.ParamName);
        Assert.False(ticket.IsDeleted);
    }

    [Fact]
    public void DeleteIsIdempotent()
    {
        Ticket ticket = CreateTicket();
        Guid originalUserId = Guid.NewGuid();

        ticket.Delete(originalUserId);

        DateTime? originalDeletedAt =
            ticket.DeletedAtUtc;
        DateTime originalUpdatedAt =
            ticket.UpdatedAtUtc;

        ticket.Delete(Guid.NewGuid());

        Assert.Equal(
            originalUserId,
            ticket.DeletedByUserId);
        Assert.Equal(
            originalDeletedAt,
            ticket.DeletedAtUtc);
        Assert.Equal(
            originalUpdatedAt,
            ticket.UpdatedAtUtc);
    }

    [Fact]
    public void DeletedTicketCannotBeChanged()
    {
        Ticket ticket = CreateTicket();

        ticket.Delete(Guid.NewGuid());

        Assert.Throws<DomainRuleException>(
            () => ticket.ChangeStatus(
                TicketStatus.InProgress));

        Assert.Throws<DomainRuleException>(
            () => ticket.UpdateDetails(
                "Changed title",
                "Changed description",
                Guid.NewGuid(),
                TicketPriority.High));
    }

    [Fact]
    public void EnsureCanReceiveCommentsOnClosedTicketThrowsDomainRuleException()
    {
        Ticket ticket = CreateClosedTicket();

        Assert.Throws<DomainRuleException>(
            () => ticket.EnsureCanReceiveComments());
    }

    [Fact]
    public void EnsureCanReceiveCommentsOnDeletedTicketThrowsDomainRuleException()
    {
        Ticket ticket = CreateTicket();

        ticket.Delete(Guid.NewGuid());

        Assert.Throws<DomainRuleException>(
            () => ticket.EnsureCanReceiveComments());
    }

    private static Ticket CreateTicket(
        string title = "Cannot access the system",
        string description =
            "The application rejects my credentials.",
        TicketPriority priority =
            TicketPriority.Medium)
    {
        return new Ticket(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            title,
            description,
            priority);
    }

    private static Ticket CreateClosedTicket()
    {
        Ticket ticket = CreateTicket();

        ticket.ChangeStatus(
            TicketStatus.InProgress);

        ticket.ChangeStatus(
            TicketStatus.Resolved);

        ticket.ChangeStatus(
            TicketStatus.Closed);

        return ticket;
    }
}
