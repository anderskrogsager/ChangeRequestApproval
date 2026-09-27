namespace ChangeRequestApproval.Web.Api;

public sealed record UpdateChangeRequestRequest(
    string? Title,
    string? Description,
    string? TechnicalImpact);
