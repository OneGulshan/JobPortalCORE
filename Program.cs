using JobPortalCORE.Models;
using JobPortalCORE.Data;
using JobPortalCORE.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection; // (Ye agar pehle se hai toh theek)

var builder = WebApplication.CreateBuilder(args);

// 1. Database & Identity Services
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<ApplicationUser>()
    .AddEntityFrameworkStores<AppDbContext>();

// 2. Controllers & Pages (Dono ko mila diya)
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews(options =>
{
    // Ye line poore project ke controllers par guard laga degi
    options.Filters.Add<GlobalExceptionFilter>();
});

// 3. Application Insights (BILKUL SAHI JAGAH PAR HAI!)
builder.Services.AddApplicationInsightsTelemetry();

// ----------------------------------------------------
var app = builder.Build();
// ----------------------------------------------------

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=JobSeeker}/{action=Index}/{id?}");

app.Run();