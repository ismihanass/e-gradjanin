using eGradjaninFBiH.Models;
using eGradjaninFBiH.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
namespace eGradjaninFBiH.Controllers;
public class AccountController(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn) : Controller
{
    [HttpGet] public IActionResult Register()=>View(new RegisterViewModel());
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if(!ModelState.IsValid)return View(model);
        var user=new ApplicationUser { UserName=model.Email, Email=model.Email, FullName=model.FullName.Trim(), EmailConfirmed=true };
        var result=await users.CreateAsync(user,model.Password);
        if(!result.Succeeded){foreach(var e in result.Errors)ModelState.AddModelError("",e.Description);return View(model);}
        await users.AddToRoleAsync(user,"Citizen"); await signIn.SignInAsync(user,isPersistent:false);
        return await RedirectToRoleHome(user);
    }
    [HttpGet] public IActionResult Login(string? returnUrl=null){ViewBag.ReturnUrl=returnUrl;return View(new LoginViewModel());}
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Login(LoginViewModel model,string? returnUrl=null)
    {
        ViewBag.ReturnUrl=returnUrl; if(!ModelState.IsValid)return View(model);
        var result=await signIn.PasswordSignInAsync(model.Email,model.Password,model.RememberMe,lockoutOnFailure:true);
        if(result.Succeeded)
        {
            var signedInUser = await users.FindByEmailAsync(model.Email);
            return signedInUser is null ? RedirectToAction("AccessDenied", "Home") : await RedirectToRoleHome(signedInUser);
        }
        ModelState.AddModelError("", "Neispravna prijava ili privremeno zaključan račun."); return View(model);
    }
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Logout(){await signIn.SignOutAsync();return RedirectToAction(nameof(Login));}

    private async Task<IActionResult> RedirectToRoleHome(ApplicationUser user)
    {
        // Resolve roles from the database rather than trusting a client-supplied role or URL.
        if (await users.IsInRoleAsync(user, "Administrator")) return RedirectToAction("Index", "Admin");
        if (await users.IsInRoleAsync(user, "Registrar")) return RedirectToAction("Index", "Registrar");
        if (await users.IsInRoleAsync(user, "Citizen")) return RedirectToAction("Index", "Home");
        return RedirectToAction("AccessDenied", "Home");
    }
}
