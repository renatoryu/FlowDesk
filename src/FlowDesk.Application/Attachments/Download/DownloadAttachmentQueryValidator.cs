using FluentValidation;

namespace FlowDesk.Application.Attachments.Download;

public sealed class DownloadAttachmentQueryValidator
    : AbstractValidator<DownloadAttachmentQuery>
{
    public DownloadAttachmentQueryValidator()
    {
        RuleFor(query => query.TicketId)
            .NotEmpty();

        RuleFor(query => query.AttachmentId)
            .NotEmpty();
    }
}
