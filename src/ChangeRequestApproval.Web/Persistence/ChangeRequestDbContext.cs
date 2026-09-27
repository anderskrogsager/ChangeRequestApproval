using ChangeRequestApproval.Web.Domain;
using Microsoft.EntityFrameworkCore;

namespace ChangeRequestApproval.Web.Persistence;

public sealed class ChangeRequestDbContext(DbContextOptions<ChangeRequestDbContext> options)
    : DbContext(options)
{
    public DbSet<ChangeRequest> ChangeRequests => Set<ChangeRequest>();
}
