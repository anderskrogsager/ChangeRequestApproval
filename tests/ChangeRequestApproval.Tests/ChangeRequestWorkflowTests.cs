using System.Net;
using System.Net.Http.Json;
using ChangeRequestApproval.Tests.Infrastructure;

namespace ChangeRequestApproval.Tests;

public sealed class ChangeRequestWorkflowTests : IClassFixture<ChangeRequestApiFactory>
{
    private readonly HttpClient _client;

    public ChangeRequestWorkflowTests(ChangeRequestApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task User_can_create_update_send_to_review_and_retrieve_a_change_request()
    {
        var createResponse = await _client.PostAsJsonAsync("/change-requests", new { });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<ChangeRequestResponse>();
        Assert.NotNull(created);
        Assert.Equal("Draft", created.Status);
        Assert.Equal("", created.Title);
        Assert.Equal("", created.Description);
        Assert.Equal("", created.TechnicalImpact);
        Assert.Equal(TimeSpan.Zero, created.CreatedAt.Offset);
        Assert.Equal($"/change-requests/{created.Id}", createResponse.Headers.Location?.ToString());

        var updateResponse = await _client.PutAsJsonAsync(
            $"/change-requests/{created.Id}",
            new
            {
                title = "Upgrade payment integration",
                description = "Move to version 3 of the provider API.",
                technicalImpact = "The payment service and its contract tests are affected."
            });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content.ReadFromJsonAsync<ChangeRequestResponse>();
        Assert.NotNull(updated);
        Assert.Equal("Draft", updated.Status);
        Assert.Equal("Upgrade payment integration", updated.Title);
        Assert.Equal("Move to version 3 of the provider API.", updated.Description);
        Assert.Equal("The payment service and its contract tests are affected.", updated.TechnicalImpact);

        var submitResponse = await _client.PostAsync(
            $"/change-requests/{created.Id}/submit-for-review",
            content: null);

        Assert.Equal(HttpStatusCode.OK, submitResponse.StatusCode);

        var submitted = await submitResponse.Content.ReadFromJsonAsync<ChangeRequestResponse>();
        Assert.NotNull(submitted);
        Assert.Equal("Submitted", submitted.Status);
        Assert.Equal("Upgrade payment integration", submitted.Title);
        Assert.Equal("Move to version 3 of the provider API.", submitted.Description);
        Assert.Equal("The payment service and its contract tests are affected.", submitted.TechnicalImpact);

        var retrieved = await _client.GetFromJsonAsync<ChangeRequestResponse>(
            $"/change-requests/{created.Id}");

        Assert.NotNull(retrieved);
        Assert.Equal(created.Id, retrieved.Id);
        Assert.Equal("Submitted", retrieved.Status);
        Assert.Equal("Upgrade payment integration", retrieved.Title);
        Assert.Equal("Move to version 3 of the provider API.", retrieved.Description);
        Assert.Equal("The payment service and its contract tests are affected.", retrieved.TechnicalImpact);
    }

    [Fact]
    public async Task User_can_approve_a_submitted_change_request()
    {
        var createResponse = await _client.PostAsJsonAsync("/change-requests", new
        {
            title = "Upgrade payment integration",
            description = "Move to version 3 of the provider API.",
            technicalImpact = "The payment service and its contract tests are affected."
        });

        var created = await createResponse.Content.ReadFromJsonAsync<ChangeRequestResponse>();
        Assert.NotNull(created);

        await _client.PostAsync($"/change-requests/{created.Id}/submit-for-review", content: null);

        var approveResponse = await _client.PostAsJsonAsync(
            $"/change-requests/{created.Id}/approve",
            new { reviewComment = "The change is ready to implement." });

        Assert.Equal(HttpStatusCode.OK, approveResponse.StatusCode);

        var approved = await approveResponse.Content.ReadFromJsonAsync<ChangeRequestResponse>();
        Assert.NotNull(approved);
        Assert.Equal("Approved", approved.Status);
        Assert.Equal("The change is ready to implement.", approved.ReviewComment);
        Assert.NotNull(approved.ReviewedAt);

        var retrieved = await _client.GetFromJsonAsync<ChangeRequestResponse>(
            $"/change-requests/{created.Id}");

        Assert.NotNull(retrieved);
        Assert.Equal("Approved", retrieved.Status);
        Assert.Equal("The change is ready to implement.", retrieved.ReviewComment);
        Assert.NotNull(retrieved.ReviewedAt);
    }

    [Fact]
    public async Task User_can_reject_a_submitted_change_request_with_a_comment()
    {
        var createResponse = await _client.PostAsJsonAsync("/change-requests", new
        {
            title = "Upgrade payment integration",
            description = "Move to version 3 of the provider API.",
            technicalImpact = "The payment service and its contract tests are affected."
        });

        var created = await createResponse.Content.ReadFromJsonAsync<ChangeRequestResponse>();
        Assert.NotNull(created);

        await _client.PostAsync($"/change-requests/{created.Id}/submit-for-review", content: null);

        var rejectResponse = await _client.PostAsJsonAsync(
            $"/change-requests/{created.Id}/reject",
            new { reviewComment = "A rollback plan is required before approval." });

        Assert.Equal(HttpStatusCode.OK, rejectResponse.StatusCode);

        var rejected = await rejectResponse.Content.ReadFromJsonAsync<ChangeRequestResponse>();
        Assert.NotNull(rejected);
        Assert.Equal("Rejected", rejected.Status);
        Assert.Equal("A rollback plan is required before approval.", rejected.ReviewComment);
        Assert.NotNull(rejected.ReviewedAt);

        var retrieved = await _client.GetFromJsonAsync<ChangeRequestResponse>(
            $"/change-requests/{created.Id}");

        Assert.NotNull(retrieved);
        Assert.Equal("Rejected", retrieved.Status);
        Assert.Equal("A rollback plan is required before approval.", retrieved.ReviewComment);
        Assert.NotNull(retrieved.ReviewedAt);
    }

    [Fact]
    public async Task User_cannot_reject_a_submitted_change_request_without_a_comment()
    {
        var createResponse = await _client.PostAsJsonAsync("/change-requests", new
        {
            title = "Upgrade payment integration",
            description = "Move to version 3 of the provider API.",
            technicalImpact = "The payment service and its contract tests are affected."
        });

        var created = await createResponse.Content.ReadFromJsonAsync<ChangeRequestResponse>();
        Assert.NotNull(created);

        await _client.PostAsync($"/change-requests/{created.Id}/submit-for-review", content: null);

        var rejectResponse = await _client.PostAsJsonAsync(
            $"/change-requests/{created.Id}/reject",
            new { reviewComment = "" });

        Assert.Equal(HttpStatusCode.BadRequest, rejectResponse.StatusCode);

        var error = await rejectResponse.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("A review comment is required when rejecting a request.", error.Error);

        var retrieved = await _client.GetFromJsonAsync<ChangeRequestResponse>(
            $"/change-requests/{created.Id}");

        Assert.NotNull(retrieved);
        Assert.Equal("Submitted", retrieved.Status);
        Assert.Null(retrieved.ReviewComment);
        Assert.Null(retrieved.ReviewedAt);
    }

    [Fact]
    public async Task User_can_update_a_draft_change_request()
    {
        var createResponse = await _client.PostAsJsonAsync("/change-requests", new { });

        var created = await createResponse.Content.ReadFromJsonAsync<ChangeRequestResponse>();
        Assert.NotNull(created);
        Assert.Equal("Draft", created.Status);

        var updateResponse = await _client.PutAsJsonAsync(
            $"/change-requests/{created.Id}",
            new
            {
                title = "Upgrade payment integration",
                description = "Move to version 3 of the provider API.",
                technicalImpact = "The payment service and its contract tests are affected."
            });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content.ReadFromJsonAsync<ChangeRequestResponse>();
        Assert.NotNull(updated);
        Assert.Equal("Draft", updated.Status);
        Assert.Equal("Upgrade payment integration", updated.Title);
        Assert.Equal("Move to version 3 of the provider API.", updated.Description);
        Assert.Equal("The payment service and its contract tests are affected.", updated.TechnicalImpact);

        var retrieved = await _client.GetFromJsonAsync<ChangeRequestResponse>(
            $"/change-requests/{created.Id}");

        Assert.NotNull(retrieved);
        Assert.Equal("Upgrade payment integration", retrieved.Title);
        Assert.Equal("Move to version 3 of the provider API.", retrieved.Description);
        Assert.Equal("The payment service and its contract tests are affected.", retrieved.TechnicalImpact);
    }

    [Fact]
    public async Task User_cannot_update_a_submitted_change_request()
    {
        var createResponse = await _client.PostAsJsonAsync("/change-requests", new
        {
            title = "Upgrade payment integration",
            description = "Move to version 3 of the provider API.",
            technicalImpact = "The payment service and its contract tests are affected."
        });

        var created = await createResponse.Content.ReadFromJsonAsync<ChangeRequestResponse>();
        Assert.NotNull(created);

        await _client.PostAsync($"/change-requests/{created.Id}/submit-for-review", content: null);

        var updateResponse = await _client.PutAsJsonAsync(
            $"/change-requests/{created.Id}",
            new
            {
                title = "A different title",
                description = "A different description.",
                technicalImpact = "A different impact."
            });

        Assert.Equal(HttpStatusCode.BadRequest, updateResponse.StatusCode);

        var error = await updateResponse.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("Only a draft can be updated.", error.Error);

        var retrieved = await _client.GetFromJsonAsync<ChangeRequestResponse>(
            $"/change-requests/{created.Id}");

        Assert.NotNull(retrieved);
        Assert.Equal("Submitted", retrieved.Status);
        Assert.Equal("Upgrade payment integration", retrieved.Title);
        Assert.Equal("Move to version 3 of the provider API.", retrieved.Description);
        Assert.Equal("The payment service and its contract tests are affected.", retrieved.TechnicalImpact);
    }

    [Fact]
    public async Task User_cannot_send_an_incomplete_draft_to_review()
    {
        var createResponse = await _client.PostAsJsonAsync("/change-requests", new { });
        var created = await createResponse.Content.ReadFromJsonAsync<ChangeRequestResponse>();
        Assert.NotNull(created);

        var sendToReviewResponse = await _client.PostAsync(
            $"/change-requests/{created.Id}/submit-for-review",
            content: null);

        Assert.Equal(HttpStatusCode.BadRequest, sendToReviewResponse.StatusCode);

        var error = await sendToReviewResponse.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal(
            "A title, description, and technical impact are required before sending to review.",
            error.Error);

        var retrieved = await _client.GetFromJsonAsync<ChangeRequestResponse>(
            $"/change-requests/{created.Id}");

        Assert.NotNull(retrieved);
        Assert.Equal("Draft", retrieved.Status);
        Assert.Equal("", retrieved.Title);
        Assert.Equal("", retrieved.Description);
        Assert.Equal("", retrieved.TechnicalImpact);
    }

    [Fact]
    public async Task User_gets_not_found_for_an_unknown_or_invalid_change_request_id()
    {
        var unknownIdResponse = await _client.GetAsync($"/change-requests/{Guid.NewGuid()}");
        var invalidIdResponse = await _client.GetAsync("/change-requests/not-a-guid");

        Assert.Equal(HttpStatusCode.NotFound, unknownIdResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, invalidIdResponse.StatusCode);
    }

    [Fact]
    public async Task User_can_view_change_requests_with_newest_first()
    {
        var firstCreateResponse = await _client.PostAsJsonAsync("/change-requests", new
        {
            title = "First request for overview test",
            description = "Created first to verify ordering.",
            technicalImpact = "Only used by the acceptance test."
        });

        var first = await firstCreateResponse.Content.ReadFromJsonAsync<ChangeRequestResponse>();
        Assert.NotNull(first);

        var secondCreateResponse = await _client.PostAsJsonAsync("/change-requests", new
        {
            title = "Second request for overview test",
            description = "Created second to verify ordering.",
            technicalImpact = "Only used by the acceptance test."
        });

        var second = await secondCreateResponse.Content.ReadFromJsonAsync<ChangeRequestResponse>();
        Assert.NotNull(second);

        var changeRequests = await _client.GetFromJsonAsync<List<ChangeRequestResponse>>(
            "/change-requests");

        Assert.NotNull(changeRequests);

        var firstIndex = changeRequests.FindIndex(changeRequest => changeRequest.Id == first.Id);
        var secondIndex = changeRequests.FindIndex(changeRequest => changeRequest.Id == second.Id);

        Assert.NotEqual(-1, firstIndex);
        Assert.NotEqual(-1, secondIndex);

        var expectedOrder = changeRequests
            .OrderByDescending(changeRequest => changeRequest.CreatedAt)
            .ThenByDescending(changeRequest => changeRequest.Id)
            .Select(changeRequest => changeRequest.Id);

        Assert.Equal(expectedOrder, changeRequests.Select(changeRequest => changeRequest.Id));
    }

    private sealed record ChangeRequestResponse(
        Guid Id,
        string Status,
        string? ReviewComment,
        DateTimeOffset? ReviewedAt,
        string Title,
        string Description,
        string TechnicalImpact,
        DateTimeOffset CreatedAt);

    private sealed record ErrorResponse(string Error);
}
