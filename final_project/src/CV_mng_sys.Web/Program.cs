using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CV_mng_sys.Core.Data;
using CV_mng_sys.Core.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireDigit = false;
    }).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

builder.Services.AddScoped<CV_mng_sys.Core.Services.AttributeLibraryService>();
builder.Services.AddScoped<CV_mng_sys.Core.Services.PositionService>();
builder.Services.AddScoped<CV_mng_sys.Core.Services.CandidateProfileService>();
builder.Services.AddScoped<CV_mng_sys.Core.Services.CvService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}

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
