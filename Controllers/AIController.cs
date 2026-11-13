using eGradjaninFBiH.Services;
using eGradjaninFBiH.ViewModels;
using Microsoft.AspNetCore.Mvc;
namespace eGradjaninFBiH.Controllers;
public class AIController(AiAssistantService ai) : Controller
{
    [HttpGet] public IActionResult Index()=>View(new AiQuestionViewModel());
    [HttpPost,ValidateAntiForgeryToken] public IActionResult Index(AiQuestionViewModel model)
    { if(!ModelState.IsValid)return View(model); ViewBag.Answer=ai.Ask(model.Question); return View(model); }
}
