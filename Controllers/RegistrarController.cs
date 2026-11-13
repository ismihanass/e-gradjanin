using eGradjaninFBiH.Data;
using eGradjaninFBiH.Models;
using eGradjaninFBiH.Services;
using eGradjaninFBiH.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.EntityFrameworkCore;
namespace eGradjaninFBiH.Controllers;
[Authorize(Roles="Registrar,Administrator")]
public class RegistrarController(ApplicationDbContext db, UserManager<ApplicationUser> users, RequestWorkflowService workflow, AuditService audit, IWebHostEnvironment env) : Controller
{
    public async Task<IActionResult> Index(string? q, string? status)
    {
        var query=db.ServiceRequests.Include(x=>x.Citizen).Include(x=>x.AdministrativeService).AsQueryable();
        if(!string.IsNullOrWhiteSpace(q)) query=query.Where(x=>x.Reference.Contains(q)||x.Citizen!.Email!.Contains(q));
        if(Enum.TryParse<RequestStatus>(status,true,out var parsed)) query=query.Where(x=>x.Status==parsed);
        ViewBag.Query=q; ViewBag.Status=status;
        return View(await query.OrderByDescending(x=>x.CreatedAt).Take(200).ToListAsync());
    }
    public async Task<IActionResult> Details(int id)
    {
        var item=await db.ServiceRequests.Include(x=>x.Citizen).Include(x=>x.AdministrativeService).Include(x=>x.Attachments).Include(x=>x.Payment).FirstOrDefaultAsync(x=>x.Id==id);
        return item is null?NotFound():View(item);
    }
    public async Task<IActionResult> Attachment(int id)
    {
        var a=await db.RequestAttachments.FirstOrDefaultAsync(x=>x.Id==id);
        if(a is null)return NotFound();
        var path=Path.Combine(env.ContentRootPath,"App_Data","uploads",a.StoredName);
        if(!System.IO.File.Exists(path))return NotFound();
        var type=a.OriginalName.EndsWith(".pdf",StringComparison.OrdinalIgnoreCase)?"application/pdf":"application/octet-stream";
        return PhysicalFile(path,type,a.OriginalName);
    }

    [HttpPost,ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(StatusUpdateViewModel model)
    {
        if(!Enum.TryParse<RequestStatus>(model.Status,true,out var next) || !await workflow.UpdateStatusAsync(model.Id,next,users.GetUserId(User)!,model.Note))
        { TempData["Error"]="Nedozvoljena promjena statusa ili zahtjev ne postoji."; return RedirectToAction(nameof(Details),new{id=model.Id}); }
        await audit.WriteAsync("REQUEST_STATUS",$"Zahtjev #{model.Id} promijenjen u {next}.");
        TempData["Success"]="Status zahtjeva je ažuriran."; return RedirectToAction(nameof(Details),new{id=model.Id});
    }
}
