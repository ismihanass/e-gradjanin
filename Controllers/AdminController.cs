using eGradjaninFBiH.Data;
using eGradjaninFBiH.Models;
using eGradjaninFBiH.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace eGradjaninFBiH.Controllers;
[Authorize(Roles="Administrator")]
public class AdminController(ApplicationDbContext db, UserManager<ApplicationUser> users, RoleManager<IdentityRole> roles, AuditService audit) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewBag.Users=await users.Users.CountAsync(); ViewBag.Requests=await db.ServiceRequests.CountAsync(); ViewBag.Open=await db.ServiceRequests.CountAsync(x=>x.Status!=RequestStatus.Completed&&x.Status!=RequestStatus.Rejected); ViewBag.Documents=await db.IssuedDocuments.CountAsync();
        ViewBag.Recent=await db.ServiceRequests.Include(x=>x.AdministrativeService).OrderByDescending(x=>x.CreatedAt).Take(8).ToListAsync(); return View();
    }
    public async Task<IActionResult> UsersList()=>View(await users.Users.OrderBy(x=>x.Email).ToListAsync());
    [HttpPost,ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeRole(string userId,string role)
    {
        if(!new[]{"Citizen","Registrar","Administrator"}.Contains(role)) return BadRequest();
        var target=await users.FindByIdAsync(userId); if(target is null)return NotFound();
        foreach(var r in await users.GetRolesAsync(target)) await users.RemoveFromRoleAsync(target,r);
        await users.AddToRoleAsync(target,role); await audit.WriteAsync("USER_ROLE_CHANGED",$"Uloga korisnika {target.Email} promijenjena u {role}."); TempData["Success"]="Uloga je promijenjena."; return RedirectToAction(nameof(UsersList));
    }
    public async Task<IActionResult> Services()=>View(await db.AdministrativeServices.OrderBy(x=>x.Name).ToListAsync());
    [HttpPost,ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleService(int id)
    { var s=await db.AdministrativeServices.FindAsync(id); if(s is null)return NotFound(); s.IsActive=!s.IsActive; await db.SaveChangesAsync(); await audit.WriteAsync("SERVICE_TOGGLED",$"Usluga {s.Name}: aktivna={s.IsActive}."); return RedirectToAction(nameof(Services)); }
    public async Task<IActionResult> Audit()=>View(await db.AuditLogs.OrderByDescending(x=>x.CreatedAt).Take(300).ToListAsync());
}
