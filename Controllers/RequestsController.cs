using eGradjaninFBiH.Data;
using eGradjaninFBiH.Models;
using eGradjaninFBiH.Services;
using eGradjaninFBiH.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace eGradjaninFBiH.Controllers;
[Authorize(Roles="Citizen,Administrator")]
public class RequestsController(ApplicationDbContext db, UserManager<ApplicationUser> users, IWebHostEnvironment env, AuditService audit) : Controller
{
    public async Task<IActionResult> Index()
    {
        var id=users.GetUserId(User)!;
        var items=await db.ServiceRequests.Include(x=>x.AdministrativeService).Include(x=>x.Payment).Where(x=>x.CitizenId==id).OrderByDescending(x=>x.CreatedAt).ToListAsync();
        ViewBag.Notifications=await db.Notifications.Where(x=>x.UserId==id).OrderByDescending(x=>x.CreatedAt).Take(8).ToListAsync();
        return View(items);
    }
    public async Task<IActionResult> Create(int? serviceId)
    {
        ViewBag.Services=await db.AdministrativeServices.Where(x=>x.IsActive).OrderBy(x=>x.Name).ToListAsync();
        return View(new RequestCreateViewModel { ServiceId=serviceId??0 });
    }
    [HttpPost,ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RequestCreateViewModel model)
    {
        ViewBag.Services=await db.AdministrativeServices.Where(x=>x.IsActive).OrderBy(x=>x.Name).ToListAsync();
        var service=await db.AdministrativeServices.FirstOrDefaultAsync(x=>x.Id==model.ServiceId && x.IsActive);
        if(service is null) ModelState.AddModelError(nameof(model.ServiceId),"Odaberite važeću uslugu.");
        if(model.Attachment is { Length: > 0 } f && (f.Length>10*1024*1024 || !new[]{".pdf",".jpg",".jpeg",".png"}.Contains(Path.GetExtension(f.FileName).ToLowerInvariant()))) ModelState.AddModelError(nameof(model.Attachment),"Dozvoljeni su PDF/JPG/PNG fajlovi do 10 MB.");
        if(!ModelState.IsValid) return View(model);
        var request=new ServiceRequest { Reference=$"EG-{DateTime.UtcNow:yyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}", CitizenId=users.GetUserId(User)!, AdministrativeServiceId=service!.Id, ApplicantNote=model.ApplicantNote.Trim(), Status=RequestStatus.Submitted };
        db.ServiceRequests.Add(request); await db.SaveChangesAsync();
        if(model.Attachment is { Length: > 0 } file)
        {
            var folder=Path.Combine(env.ContentRootPath,"App_Data","uploads"); Directory.CreateDirectory(folder);
            var stored=Guid.NewGuid().ToString("N")+Path.GetExtension(file.FileName).ToLowerInvariant();
            await using var stream=System.IO.File.Create(Path.Combine(folder,stored)); await file.CopyToAsync(stream);
            db.RequestAttachments.Add(new RequestAttachment { ServiceRequestId=request.Id, OriginalName=Path.GetFileName(file.FileName), StoredName=stored }); await db.SaveChangesAsync();
        }
        if(service.Fee>0) db.Payments.Add(new Payment { ServiceRequestId=request.Id, Amount=service.Fee, Status="Pending", TransactionReference="PENDING" });
        db.Notifications.Add(new UserNotification { UserId=request.CitizenId, Message=$"Zahtjev {request.Reference} je uspješno zaprimljen." }); await db.SaveChangesAsync();
        await audit.WriteAsync("REQUEST_CREATED",$"Zahtjev {request.Reference} je kreiran.");
        return RedirectToAction(nameof(Details),new{id=request.Id});
    }
    public async Task<IActionResult> Details(int id)
    {
        var uid=users.GetUserId(User)!;
        var item=await db.ServiceRequests.Include(x=>x.AdministrativeService).Include(x=>x.Attachments).Include(x=>x.Payment).Include(x=>x.IssuedDocument).FirstOrDefaultAsync(x=>x.Id==id && x.CitizenId==uid);
        return item is null?NotFound():View(item);
    }
    [HttpPost,ValidateAntiForgeryToken]
    public async Task<IActionResult> SimulatePayment(int id)
    {
        var uid=users.GetUserId(User)!; var payment=await db.Payments.Include(x=>x.ServiceRequest).FirstOrDefaultAsync(x=>x.ServiceRequestId==id && x.ServiceRequest!.CitizenId==uid);
        if(payment is null) return NotFound();
        if(payment.Status!="Paid") { payment.Status="Paid"; payment.PaidAt=DateTime.UtcNow; payment.TransactionReference=$"DEMO-{Guid.NewGuid().ToString("N")[..10].ToUpperInvariant()}"; await db.SaveChangesAsync(); await audit.WriteAsync("DEMO_PAYMENT",$"Simulirana uplata za {payment.ServiceRequest!.Reference}."); }
        TempData["Success"]="Demo uplata je evidentirana. Nije izvršena stvarna naplata.";
        return RedirectToAction(nameof(Details),new{id});
    }
    public async Task<IActionResult> Attachment(int id)
    {
        var uid=users.GetUserId(User)!; var a=await db.RequestAttachments.Include(x=>x.ServiceRequest).FirstOrDefaultAsync(x=>x.Id==id && x.ServiceRequest!.CitizenId==uid);
        if(a is null) return NotFound(); var path=Path.Combine(env.ContentRootPath,"App_Data","uploads",a.StoredName); if(!System.IO.File.Exists(path)) return NotFound();
        return PhysicalFile(path, a.OriginalName.EndsWith(".pdf",StringComparison.OrdinalIgnoreCase)?"application/pdf":"application/octet-stream",a.OriginalName);
    }
}
