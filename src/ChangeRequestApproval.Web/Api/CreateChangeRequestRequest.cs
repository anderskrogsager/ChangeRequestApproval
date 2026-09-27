namespace ChangeRequestApproval.Web.Api;

public sealed record CreateChangeRequestRequest(
    string? Title,
    string? Description,
    string? TechnicalImpact);
