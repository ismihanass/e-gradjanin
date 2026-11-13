using eGradjaninFBiH.Data;
using eGradjaninFBiH.Models;
using eGradjaninFBiH.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var connection = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=egradjanin.db";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (connection.Contains("Host=", StringComparison.OrdinalIgnoreCase) || connection.Contains("Server=", StringComparison.OrdinalIgnoreCase))
        options.UseNpgsql(connection);
    else options.UseSqlite(connection);
});
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.User.RequireUniqueEmail = true;
}).AddRoles<IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.ConfigureApplicationCookie(o => { o.LoginPath = "/Account/Login"; o.AccessDeniedPath = "/Home/AccessDenied"; o.Cookie.HttpOnly = true; o.Cookie.SameSite = SameSiteMode.Lax; o.SlidingExpiration = true; });
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddScoped<RequestWorkflowService>();
builder.Services.AddScoped<PdfDocumentService>();
builder.Services.AddSingleton<AiAssistantService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuditService>();
builder.Services.AddAntiforgery(o => o.HeaderName = "RequestVerificationToken");

var app = builder.Build();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Home/Error"); app.UseHsts(); }
app.UseHttpsRedirection(); app.UseStaticFiles(); app.UseRouting(); app.UseAuthentication(); app.UseAuthorization();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.EnsureCreatedAsync(); // Demo bootstrap. Use EF migrations for production deployments.
    var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    foreach (var role in new[] { "Citizen", "Registrar", "Administrator" })
        if (!await roles.RoleExistsAsync(role)) await roles.CreateAsync(new IdentityRole(role));
    var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    await SeedUser(users, "admin@egradjanin.demo", "Admin123!", "Demo administrator", "Administrator");
    await SeedUser(users, "sluzbenik@egradjanin.demo", "Sluzbenik123!", "Demo službenik", "Registrar");
    await SeedUser(users, "gradjanin@egradjanin.demo", "Gradjanin123!", "Demo građanin", "Citizen");
    if (!await db.AdministrativeServices.AnyAsync())
    {
        db.AdministrativeServices.AddRange(
            new AdministrativeService { Name="Izvod iz matične knjige rođenih", Category="Matične evidencije", Description="Podnošenje zahtjeva za izdavanje izvoda iz matične knjige rođenih.", RequiredDocuments="Identifikacioni dokument; dokaz o uplati takse (ako je primjenjivo)", Fee=8, EstimatedDays=3 },
            new AdministrativeService { Name="Uvjerenje o državljanstvu", Category="Matične evidencije", Description="Informativni demo postupak za zahtjev za uvjerenje o državljanstvu.", RequiredDocuments="Identifikacioni dokument; dokaz o uplati takse (ako je primjenjivo)", Fee=10, EstimatedDays=3 },
            new AdministrativeService { Name="Uvjerenje o prebivalištu", Category="Lične evidencije", Description="Podnošenje zahtjeva za uvjerenje o prebivalištu/boravištu.", RequiredDocuments="Identifikacioni dokument", Fee=5, EstimatedDays=2 },
            new AdministrativeService { Name="Ostala službena potvrda", Category="Ostalo", Description="Demo obrazac za ostale administrativne potvrde.", RequiredDocuments="Identifikacioni dokument; dokumentacija prema vrsti potvrde", Fee=0, EstimatedDays=5 });
        await db.SaveChangesAsync();
    }
}
app.Run();

static async Task SeedUser(UserManager<ApplicationUser> manager, string email, string password, string name, string role)
{
    var user = await manager.FindByEmailAsync(email);
    if (user is null) { user = new ApplicationUser { UserName=email, Email=email, EmailConfirmed=true, FullName=name }; var result = await manager.CreateAsync(user, password); if (!result.Succeeded) throw new Exception(string.Join("; ", result.Errors.Select(e=>e.Description))); }
    if (!await manager.IsInRoleAsync(user, role)) await manager.AddToRoleAsync(user, role);
}
