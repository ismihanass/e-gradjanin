using eGradjaninFBiH.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace eGradjaninFBiH.Data;
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<AdministrativeService> AdministrativeServices => Set<AdministrativeService>();
    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();
    public DbSet<RequestAttachment> RequestAttachments => Set<RequestAttachment>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<IssuedDocument> IssuedDocuments => Set<IssuedDocument>();
    public DbSet<UserNotification> Notifications => Set<UserNotification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.Entity<ServiceRequest>().HasIndex(x=>x.Reference).IsUnique();
        b.Entity<IssuedDocument>().HasIndex(x=>x.VerificationToken).IsUnique();
        b.Entity<Payment>().HasIndex(x=>x.ServiceRequestId).IsUnique();
        b.Entity<IssuedDocument>().HasIndex(x=>x.ServiceRequestId).IsUnique();
        b.Entity<ServiceRequest>().Property(x=>x.Status).HasConversion<string>();
        b.Entity<AdministrativeService>().Property(x=>x.Fee).HasPrecision(10,2);
    }
}
