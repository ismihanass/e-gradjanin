using System.Diagnostics;
using eGradjaninFBiH.Data;
using eGradjaninFBiH.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace eGradjaninFBiH.Controllers;
public class HomeController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        if (User.Identity?.IsAuthenticated != true)
            return RedirectToAction("Login", "Account");

        // Send authenticated users to the panel belonging to their server-verified role.
        if (User.IsInRole("Administrator")) return RedirectToAction("Index", "Admin");
        if (User.IsInRole("Registrar")) return RedirectToAction("Index", "Registrar");
        if (!User.IsInRole("Citizen")) return RedirectToAction("AccessDenied");

        ViewBag.Services=await db.AdministrativeServices.Where(x=>x.IsActive).OrderBy(x=>x.Category).ToListAsync();
        if (User.Identity?.IsAuthenticated==true)
        {
            var id=User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
            ViewBag.MyCount=await db.ServiceRequests.CountAsync(x=>x.CitizenId==id);
            ViewBag.PendingCount=await db.ServiceRequests.CountAsync(x=>x.CitizenId==id && x.Status!=RequestStatus.Completed && x.Status!=RequestStatus.Rejected);
        }
        return View();
    }
    public IActionResult Privacy()=>View();
    public IActionResult AccessDenied()=>View();
    public IActionResult Error()=>View();
}
