using EvoluxIoT.Web.Data;
using EvoluxIoT.Web.Hubs;
using EvoluxIoT.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MQTTnet.AspNetCore;
using MQTTnet.Extensions.ManagedClient;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddMqttClient();
builder.Services.AddSignalR();

// Configure proxies
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    options.KnownNetworks.Add(new IPNetwork(IPAddress.Loopback, 8));
    options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("::1"), 128));

    options.KnownProxies.Add(IPAddress.Loopback);
    options.KnownProxies.Add(IPAddress.Parse("::1"));
});

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Strict;
    options.Secure = CookieSecurePolicy.Always;
});


builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication().AddGoogle(googleOptions =>
{
    googleOptions.ClientId = "397410433797-1ald780afhjnjq2n1ddv23rh5o6bk3rt.apps.googleusercontent.com";
    googleOptions.ClientSecret = "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";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint(); 
    app.UseHttpsRedirection();
}
else
{
    app.UseExceptionHandler("/Home/Error");

    app.UseForwardedHeaders();
    app.Use((ctx, next) =>
    {
        ctx.Request.Host = new HostString("portal.evoluxiot.pt");
        ctx.Request.Scheme = "https";
        return next();
    });
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    // app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();
app.MapHub<SynapseHub>("/synapsehub");

app.Run();
