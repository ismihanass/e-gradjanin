using eGradjaninFBiH.Data;
using eGradjaninFBiH.Models;
using Microsoft.AspNetCore.Identity;
namespace eGradjaninFBiH.Services;
public class AuditService(ApplicationDbContext db, IHttpContextAccessor accessor)
{
    public async Task WriteAsync(string action, string details)
    {
        db.AuditLogs.Add(new AuditLog { ActorId=accessor.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, Action=action, Details=details });
        await db.SaveChangesAsync();
    }
}
