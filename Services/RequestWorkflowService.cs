using eGradjaninFBiH.Data;
using eGradjaninFBiH.Models;
using Microsoft.EntityFrameworkCore;
namespace eGradjaninFBiH.Services;
public class RequestWorkflowService(ApplicationDbContext db)
{
    public async Task<bool> UpdateStatusAsync(int id, RequestStatus next, string registrarId, string? note)
    {
        var item = await db.ServiceRequests.Include(x=>x.Citizen).FirstOrDefaultAsync(x=>x.Id==id);
        if (item is null) return false;
        var allowed = item.Status switch
        {
            RequestStatus.Submitted => new[]{RequestStatus.UnderReview, RequestStatus.AdditionalInformationRequired, RequestStatus.Rejected},
            RequestStatus.UnderReview => new[]{RequestStatus.AdditionalInformationRequired, RequestStatus.Approved, RequestStatus.Rejected},
            RequestStatus.AdditionalInformationRequired => new[]{RequestStatus.UnderReview, RequestStatus.Rejected},
            RequestStatus.Approved => new[]{RequestStatus.Completed},
            _ => Array.Empty<RequestStatus>()
        };
        if (!allowed.Contains(next)) return false;
        item.Status=next; item.RegistrarId=registrarId; item.RegistrarNote=note; item.UpdatedAt=DateTime.UtcNow;
        db.Notifications.Add(new UserNotification { UserId=item.CitizenId, Message=$"Zahtjev {item.Reference}: status je promijenjen na {StatusText(next)}." });
        if (next==RequestStatus.Approved && item.IssuedDocument is null)
            db.IssuedDocuments.Add(new IssuedDocument { ServiceRequestId=item.Id, DocumentReference=$"DEMO-{Guid.NewGuid():N}"[..17].ToUpperInvariant(), VerificationToken=Guid.NewGuid().ToString("N") });
        await db.SaveChangesAsync(); return true;
    }
    public static string StatusText(RequestStatus s) => s switch { RequestStatus.Submitted=>"Zaprimljen", RequestStatus.UnderReview=>"U obradi", RequestStatus.AdditionalInformationRequired=>"Potrebna dopuna", RequestStatus.Approved=>"Odobren", RequestStatus.Rejected=>"Odbijen", _=>"Završen" };
}
