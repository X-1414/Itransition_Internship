using Microsoft.EntityFrameworkCore;
using AdminDashboard.Models;
using AdminDashboard.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.CodeAnalysis.Options;

var builder = WebApplication.CreateBuilder(args);

// render inject the port to listen
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// postgresql
builder.Services.AddDbContext<AppDbContext>(options=>options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

//services
builder.Services.AddScoped<UserStatusFilter>();
builder.Services.AddControllersWithViews( options => {options.Filters.Add<UserStatusFilter>();} );
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});
// auth login
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(24);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

var app = builder.Build();

app.UseForwardedHeaders();

using (var scope= app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Account/Login");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RENDER")))
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();