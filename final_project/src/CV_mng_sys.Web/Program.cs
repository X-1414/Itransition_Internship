using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CV_mng_sys.Core.Data;
using CV_mng_sys.Core.Entities;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using AspNet.Security.OAuth.GitHub;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddHttpClient<CV_mng_sys.Core.Services.SalesforceService>();
builder.Services.AddHttpClient<CV_mng_sys.Core.Services.SupportTicketService>();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireDigit = false;
        options.Lockout.AllowedForNewUsers = true;
    }).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
builder.Services.AddAuthentication().AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
    }).AddGitHub(options =>
    {
        options.ClientId = builder.Configuration["Authentication:GitHub:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"]!;
        options.Scope.Add("user:email");
    });
builder.Services.AddLocalization(options=>options.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews().AddViewLocalization().AddDataAnnotationsLocalization();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "en-US", "ru-RU" };
    options.SetDefaultCulture(supportedCultures[0]);
    options.AddSupportedCultures(supportedCultures);
    options.AddSupportedUICultures(supportedCultures);
    options.RequestCultureProviders = new List<IRequestCultureProvider>
    {
        new CookieRequestCultureProvider(),
        new AcceptLanguageHeaderRequestCultureProvider()
    };
});
builder.Services.AddScoped<CV_mng_sys.Core.Services.AttributeLibraryService>();
builder.Services.AddScoped<CV_mng_sys.Core.Services.PositionService>();
builder.Services.AddScoped<CV_mng_sys.Core.Services.CandidateProfileService>();
builder.Services.AddScoped<CV_mng_sys.Core.Services.CvService>();
builder.Services.AddScoped<CV_mng_sys.Core.Services.ProjectService>();
builder.Services.AddScoped<CV_mng_sys.Core.Services.DiscussionService>();
builder.Services.AddScoped<CV_mng_sys.Core.Services.UserStatsService>();
builder.Services.AddScoped<CV_mng_sys.Core.Services.UserManagementService>();
builder.Services.ConfigureApplicationCookie(options =>{ options.AccessDeniedPath = "/Home/Index"; });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}
app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}").WithStaticAssets();

app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await DataSeeder.SeedRolesAsync(roleManager);
}

using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var adminEmail = builder.Configuration["SeedAdmin:Email"];
    if (!string.IsNullOrEmpty(adminEmail))
    {
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser != null && !await userManager.IsInRoleAsync(adminUser, RoleNames.Administrator))
        {
            await userManager.AddToRoleAsync(adminUser, RoleNames.Administrator);
        }
    }
}

app.Run();
