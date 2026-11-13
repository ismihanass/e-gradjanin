using eGradjaninFBiH.Data;
using eGradjaninFBiH.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace eGradjaninFBiH.Controllers;
public class DocumentsController(ApplicationDbContext db, PdfDocumentService pdf) : Controller
{
    [AllowAnonymous, HttpGet("/verify/{token}")]
    public async Task<IActionResult> Verify(string token)
    {
        var doc=await db.IssuedDocuments.Include(x=>x.ServiceRequest).ThenInclude(x=>x!.AdministrativeService).FirstOrDefaultAsync(x=>x.VerificationToken==token);
        return doc is null?View("Invalid"):View("Verify",doc);
    }
    [Authorize(Roles="Citizen,Administrator")]
    public async Task<IActionResult> Download(int id)
    {
        var uid=User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        var doc=await db.IssuedDocuments.Include(x=>x.ServiceRequest).ThenInclude(x=>x!.AdministrativeService).FirstOrDefaultAsync(x=>x.Id==id && x.ServiceRequest!.CitizenId==uid);
        if(doc is null)return NotFound();
        var url=$"{Request.Scheme}://{Request.Host}/verify/{doc.VerificationToken}";
        return File(pdf.Create(doc.ServiceRequest!,doc,url),"application/pdf",$"{doc.DocumentReference}.pdf");
    }
}
