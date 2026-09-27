namespace ChangeRequestApproval.Web.Domain;

public sealed class ChangeRequest
{
    public ChangeRequest()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTimeOffset.UtcNow;
        Status = ChangeRequestStatus.Draft;
    }

    public Guid Id { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public string TechnicalImpact { get; private set; } = string.Empty;

    public string? ReviewComment { get; private set; }

    public DateTimeOffset? ReviewedAt { get; private set; }

    public ChangeRequestStatus Status { get; private set; }

    public void UpdateDetails(string title, string description, string technicalImpact)
    {
        if (Status != ChangeRequestStatus.Draft)
        {
            throw new InvalidOperationException("Only a draft can be updated.");
        }

        Title = title;
        Description = description;
        TechnicalImpact = technicalImpact;
    }

    public void SubmitForReview()
    {
        if (Status != ChangeRequestStatus.Draft)
        {
            throw new InvalidOperationException("Only a draft can be sent to review.");
        }

        if (string.IsNullOrWhiteSpace(Title)
            || string.IsNullOrWhiteSpace(Description)
            || string.IsNullOrWhiteSpace(TechnicalImpact))
        {
            throw new InvalidOperationException(
                "A title, description, and technical impact are required before sending to review.");
        }

        Status = ChangeRequestStatus.Submitted;
    }

    public void Approve(string? reviewComment = null)
    {
        if (Status != ChangeRequestStatus.Submitted)
        {
            throw new InvalidOperationException("Only a submitted request can be approved.");
        }

        ReviewComment = string.IsNullOrWhiteSpace(reviewComment)
            ? null
            : reviewComment;
        ReviewedAt = DateTimeOffset.UtcNow;
        Status = ChangeRequestStatus.Approved;
    }

    public void Reject(string reviewComment)
    {
        if (Status != ChangeRequestStatus.Submitted)
        {
            throw new InvalidOperationException("Only a submitted request can be rejected.");
        }

        if (string.IsNullOrWhiteSpace(reviewComment))
        {
            throw new InvalidOperationException("A review comment is required when rejecting a request.");
        }

        ReviewComment = reviewComment;
        ReviewedAt = DateTimeOffset.UtcNow;
        Status = ChangeRequestStatus.Rejected;
    }
}
