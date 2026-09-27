using ChangeRequestApproval.Web.Domain;

namespace ChangeRequestApproval.Tests;

public class ChangeRequestTests
{
    [Fact]
    public void New_change_request_starts_as_draft_with_an_id_and_utc_creation_time()
    {
        var changeRequest = new ChangeRequest();

        Assert.Equal(ChangeRequestStatus.Draft, changeRequest.Status);
        Assert.NotEqual(Guid.Empty, changeRequest.Id);
        Assert.Equal(TimeSpan.Zero, changeRequest.CreatedAt.Offset);
    }

    [Fact]
    public void Draft_can_be_incomplete()
    {
        var changeRequest = new ChangeRequest();

        changeRequest.UpdateDetails(title: "", description: "", technicalImpact: "");

        Assert.Equal(ChangeRequestStatus.Draft, changeRequest.Status);
        Assert.Equal("", changeRequest.Title);
        Assert.Equal("", changeRequest.Description);
        Assert.Equal("", changeRequest.TechnicalImpact);
    }

    [Fact]
    public void Draft_can_be_updated()
    {
        var changeRequest = new ChangeRequest();

        changeRequest.UpdateDetails(
            title: "Upgrade payment integration",
            description: "Move to version 3 of the provider API.",
            technicalImpact: "The payment service and its contract tests are affected.");

        Assert.Equal("Upgrade payment integration", changeRequest.Title);
        Assert.Equal("Move to version 3 of the provider API.", changeRequest.Description);
        Assert.Equal(
            "The payment service and its contract tests are affected.",
            changeRequest.TechnicalImpact);
        Assert.Equal(ChangeRequestStatus.Draft, changeRequest.Status);
    }

    [Fact]
    public void Complete_draft_can_be_sent_to_review()
    {
        var changeRequest = CreateCompleteDraft();

        changeRequest.SubmitForReview();

        Assert.Equal(ChangeRequestStatus.Submitted, changeRequest.Status);
    }

    [Theory]
    [InlineData("", "A description", "A technical impact")]
    [InlineData("A title", "", "A technical impact")]
    [InlineData("A title", "A description", "")]
    [InlineData("   ", "A description", "A technical impact")]
    [InlineData("A title", "   ", "A technical impact")]
    [InlineData("A title", "A description", "   ")]
    public void Draft_with_a_missing_or_whitespace_required_field_cannot_be_sent_to_review(
        string title,
        string description,
        string technicalImpact)
    {
        var changeRequest = new ChangeRequest();
        changeRequest.UpdateDetails(title, description, technicalImpact);

        Assert.Throws<InvalidOperationException>(() => changeRequest.SubmitForReview());
        Assert.Equal(ChangeRequestStatus.Draft, changeRequest.Status);
    }

    [Fact]
    public void Submitted_request_cannot_be_updated_and_keeps_its_details()
    {
        var changeRequest = CreateSubmittedChangeRequest();

        Assert.Throws<InvalidOperationException>(() => changeRequest.UpdateDetails(
            title: "A different title",
            description: "A different description",
            technicalImpact: "A different technical impact"));

        Assert.Equal(ChangeRequestStatus.Submitted, changeRequest.Status);
        Assert.Equal("Upgrade payment integration", changeRequest.Title);
        Assert.Equal("Move to version 3 of the provider API.", changeRequest.Description);
        Assert.Equal(
            "The payment service and its contract tests are affected.",
            changeRequest.TechnicalImpact);
    }

    [Fact]
    public void Submitted_request_cannot_be_sent_to_review_again()
    {
        var changeRequest = CreateSubmittedChangeRequest();

        Assert.Throws<InvalidOperationException>(() => changeRequest.SubmitForReview());

        Assert.Equal(ChangeRequestStatus.Submitted, changeRequest.Status);
    }

    [Fact]
    public void Submitted_request_can_be_approved()
    {
        var changeRequest = CreateSubmittedChangeRequest();

        changeRequest.Approve();

        Assert.Equal(ChangeRequestStatus.Approved, changeRequest.Status);
        Assert.Null(changeRequest.ReviewComment);
        Assert.NotNull(changeRequest.ReviewedAt);
        Assert.Equal(TimeSpan.Zero, changeRequest.ReviewedAt!.Value.Offset);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Submitted_request_approved_with_blank_review_comment_has_no_review_comment(string reviewComment)
    {
        var changeRequest = CreateSubmittedChangeRequest();

        changeRequest.Approve(reviewComment);

        Assert.Null(changeRequest.ReviewComment);
    }

    [Fact]
    public void Draft_cannot_be_approved()
    {
        var changeRequest = CreateCompleteDraft();

        Assert.Throws<InvalidOperationException>(() => changeRequest.Approve());

        Assert.Equal(ChangeRequestStatus.Draft, changeRequest.Status);
        Assert.Null(changeRequest.ReviewedAt);
    }

    [Fact]
    public void Submitted_request_can_be_approved_with_a_review_comment()
    {
        var changeRequest = CreateSubmittedChangeRequest();

        changeRequest.Approve("The migration plan is sufficient.");

        Assert.Equal(ChangeRequestStatus.Approved, changeRequest.Status);
        Assert.Equal("The migration plan is sufficient.", changeRequest.ReviewComment);
        Assert.NotNull(changeRequest.ReviewedAt);
        Assert.Equal(TimeSpan.Zero, changeRequest.ReviewedAt!.Value.Offset);
    }

    [Fact]
    public void Submitted_request_can_be_rejected_with_a_review_comment()
    {
        var changeRequest = CreateSubmittedChangeRequest();

        changeRequest.Reject("The payment provider has not published a migration guide yet.");

        Assert.Equal(ChangeRequestStatus.Rejected, changeRequest.Status);
        Assert.Equal(
            "The payment provider has not published a migration guide yet.",
            changeRequest.ReviewComment);
        Assert.NotNull(changeRequest.ReviewedAt);
        Assert.Equal(TimeSpan.Zero, changeRequest.ReviewedAt!.Value.Offset);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Submitted_request_cannot_be_rejected_without_a_review_comment(string reviewComment)
    {
        var changeRequest = CreateSubmittedChangeRequest();

        Assert.Throws<InvalidOperationException>(() => changeRequest.Reject(reviewComment));

        Assert.Equal(ChangeRequestStatus.Submitted, changeRequest.Status);
        Assert.Null(changeRequest.ReviewComment);
        Assert.Null(changeRequest.ReviewedAt);
    }

    [Fact]
    public void Draft_cannot_be_rejected()
    {
        var changeRequest = CreateCompleteDraft();

        Assert.Throws<InvalidOperationException>(() => changeRequest.Reject("A review comment."));

        Assert.Equal(ChangeRequestStatus.Draft, changeRequest.Status);
        Assert.Null(changeRequest.ReviewedAt);
    }

    [Fact]
    public void Approved_request_cannot_be_changed_or_reviewed_again()
    {
        var changeRequest = CreateApprovedChangeRequest();
        var reviewedAt = changeRequest.ReviewedAt;

        Assert.Throws<InvalidOperationException>(() => changeRequest.UpdateDetails(
            title: "A different title",
            description: "A different description",
            technicalImpact: "A different technical impact"));
        Assert.Throws<InvalidOperationException>(() => changeRequest.SubmitForReview());
        Assert.Throws<InvalidOperationException>(() => changeRequest.Approve());
        Assert.Throws<InvalidOperationException>(() => changeRequest.Reject("A review comment."));

        Assert.Equal(ChangeRequestStatus.Approved, changeRequest.Status);
        Assert.Equal("Upgrade payment integration", changeRequest.Title);
        Assert.Equal("Move to version 3 of the provider API.", changeRequest.Description);
        Assert.Equal(
            "The payment service and its contract tests are affected.",
            changeRequest.TechnicalImpact);
        Assert.Equal("A review comment.", changeRequest.ReviewComment);
        Assert.Equal(reviewedAt, changeRequest.ReviewedAt);
    }

    [Fact]
    public void Rejected_request_cannot_be_changed_or_reviewed_again()
    {
        var changeRequest = CreateRejectedChangeRequest();
        var reviewedAt = changeRequest.ReviewedAt;

        Assert.Throws<InvalidOperationException>(() => changeRequest.UpdateDetails(
            title: "A different title",
            description: "A different description",
            technicalImpact: "A different technical impact"));
        Assert.Throws<InvalidOperationException>(() => changeRequest.SubmitForReview());
        Assert.Throws<InvalidOperationException>(() => changeRequest.Approve());
        Assert.Throws<InvalidOperationException>(() => changeRequest.Reject("A different review comment."));

        Assert.Equal(ChangeRequestStatus.Rejected, changeRequest.Status);
        Assert.Equal("Upgrade payment integration", changeRequest.Title);
        Assert.Equal("Move to version 3 of the provider API.", changeRequest.Description);
        Assert.Equal(
            "The payment service and its contract tests are affected.",
            changeRequest.TechnicalImpact);
        Assert.Equal("A review comment.", changeRequest.ReviewComment);
        Assert.Equal(reviewedAt, changeRequest.ReviewedAt);
    }

    private static ChangeRequest CreateCompleteDraft()
    {
        var changeRequest = new ChangeRequest();
        changeRequest.UpdateDetails(
            title: "Upgrade payment integration",
            description: "Move to version 3 of the provider API.",
            technicalImpact: "The payment service and its contract tests are affected.");

        return changeRequest;
    }

    private static ChangeRequest CreateSubmittedChangeRequest()
    {
        var changeRequest = CreateCompleteDraft();
        changeRequest.SubmitForReview();

        return changeRequest;
    }

    private static ChangeRequest CreateApprovedChangeRequest()
    {
        var changeRequest = CreateSubmittedChangeRequest();
        changeRequest.Approve("A review comment.");

        return changeRequest;
    }

    private static ChangeRequest CreateRejectedChangeRequest()
    {
        var changeRequest = CreateSubmittedChangeRequest();
        changeRequest.Reject("A review comment.");

        return changeRequest;
    }
}
