using ChangeRequestApproval.Web.Api;
using ChangeRequestApproval.Web.Domain;
using ChangeRequestApproval.Web.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ChangeRequestDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("ChangeRequestDatabase")));
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var database = scope.ServiceProvider.GetRequiredService<ChangeRequestDbContext>();
    database.Database.EnsureCreated();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapPost("/change-requests", async (
    CreateChangeRequestRequest request,
    ChangeRequestDbContext database) =>
{
    var changeRequest = new ChangeRequest();
    changeRequest.UpdateDetails(
        request.Title ?? string.Empty,
        request.Description ?? string.Empty,
        request.TechnicalImpact ?? string.Empty);

    database.ChangeRequests.Add(changeRequest);
    await database.SaveChangesAsync();

    return Results.Created($"/change-requests/{changeRequest.Id}", changeRequest);
});

app.MapGet("/change-requests", async (ChangeRequestDbContext database) =>
{
    var changeRequests = await database.ChangeRequests.ToListAsync();

    var orderedChangeRequests = changeRequests
        .OrderByDescending(changeRequest => changeRequest.CreatedAt)
        .ThenByDescending(changeRequest => changeRequest.Id);

    return Results.Ok(orderedChangeRequests);
});

app.MapGet("/change-requests/{id:guid}", async (
    Guid id,
    ChangeRequestDbContext database) =>
{
    var changeRequest = await database.ChangeRequests.FindAsync(id);

    return changeRequest is null
        ? Results.NotFound()
        : Results.Ok(changeRequest);
});

app.MapPut("/change-requests/{id:guid}", async (
    Guid id,
    UpdateChangeRequestRequest request,
    ChangeRequestDbContext database) =>
{
    var changeRequest = await database.ChangeRequests.FindAsync(id);
    if (changeRequest is null)
    {
        return Results.NotFound();
    }

    try
    {
        changeRequest.UpdateDetails(
            request.Title ?? string.Empty,
            request.Description ?? string.Empty,
            request.TechnicalImpact ?? string.Empty);
        await database.SaveChangesAsync();

        return Results.Ok(changeRequest);
    }
    catch (InvalidOperationException exception)
    {
        return Results.BadRequest(new { error = exception.Message });
    }
});

app.MapPost("/change-requests/{id:guid}/submit-for-review", async (
    Guid id,
    ChangeRequestDbContext database) =>
{
    var changeRequest = await database.ChangeRequests.FindAsync(id);
    if (changeRequest is null)
    {
        return Results.NotFound();
    }

    try
    {
        changeRequest.SubmitForReview();
        await database.SaveChangesAsync();

        return Results.Ok(changeRequest);
    }
    catch (InvalidOperationException exception)
    {
        return Results.BadRequest(new { error = exception.Message });
    }
});

app.MapPost("/change-requests/{id:guid}/approve", async (
    Guid id,
    ApproveChangeRequestRequest? request,
    ChangeRequestDbContext database) =>
{
    var changeRequest = await database.ChangeRequests.FindAsync(id);
    if (changeRequest is null)
    {
        return Results.NotFound();
    }

    try
    {
        changeRequest.Approve(request?.ReviewComment);
        await database.SaveChangesAsync();

        return Results.Ok(changeRequest);
    }
    catch (InvalidOperationException exception)
    {
        return Results.BadRequest(new { error = exception.Message });
    }
});

app.MapPost("/change-requests/{id:guid}/reject", async (
    Guid id,
    RejectChangeRequestRequest request,
    ChangeRequestDbContext database) =>
{
    var changeRequest = await database.ChangeRequests.FindAsync(id);
    if (changeRequest is null)
    {
        return Results.NotFound();
    }

    try
    {
        changeRequest.Reject(request.ReviewComment ?? string.Empty);
        await database.SaveChangesAsync();

        return Results.Ok(changeRequest);
    }
    catch (InvalidOperationException exception)
    {
        return Results.BadRequest(new { error = exception.Message });
    }
});

app.Run();

// Makes the compiler-generated Program type accessible to WebApplicationFactory<Program> in workflow tests.
public partial class Program;
