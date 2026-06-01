using Hangfire;
using JobPortalCORE.Data;
using JobPortalCORE.Filters;
using JobPortalCORE.Models;
using JobPortalCORE.Services;
using Microsoft.EntityFrameworkCore;
using JobPortalCORE.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options => 
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<ApplicationUser>().AddEntityFrameworkStores<AppDbContext>();
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews(options =>
{
    // Ye line poore project ke controllers par guard laga degi
    options.Filters.Add<GlobalExceptionFilter>();
});

// Azure Application Insights Telemetry ko runtime par chalu karne ke liye
builder.Services.AddApplicationInsightsTelemetry();

// 👇 Hangfire Service register karo aur use SQL Server database se jodo
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

// Background server chalu karo jo tasks ko execute karega
builder.Services.AddHangfireServer();

// 👇 Ye tere background postman (Receiver) ko chalu kar dega
//builder.Services.AddHostedService<ServiceBusReceiverWorker>();

//// Ye line SignalR ko local ki jagah seedha Azure Cloud par bhej degi
//builder.Services.AddSignalR()
//       .AddAzureSignalR(builder.Configuration.GetConnectionString("AzureSignalRConnectionString"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseHangfireDashboard();
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=JobSeeker}/{action=Index}/{id?}");
//app.MapHub<NotificationHub>("/notificationHub");
app.Run();
