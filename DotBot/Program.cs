using DotBot.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Vkontakte;
using DotBot.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.Cookies;
using DotBot.DLA;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{    
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
}).AddEntityFrameworkStores<ApplicationDbContext>();




builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<vkContext>(
    options => options.UseSqlServer("name=ConnectionStrings:DefaultConnection", o => o.EnableRetryOnFailure())
    ); ;

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(VkontakteAuthenticationDefaults.AuthenticationScheme)
.AddVkontakte(options =>
    {
        options.ClientId = builder.Configuration["Config:ClientId"];
        options.ClientSecret = builder.Configuration["Config:SecureKey"];
		options.CallbackPath = new PathString("/get-code-vkontakte");
        options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "my-id");
    })
    .AddCookie();






var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();

}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    //app.UseHsts();
}
app.UseCookiePolicy(new CookiePolicyOptions()
{
    MinimumSameSitePolicy = SameSiteMode.Strict
});

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
//app.UseVkontakteAuthentication("{AppId}", "{AppSecret}", "{PERMISSIONS}");

app.UseAuthentication();
app.UseAuthorization();



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.UseEndpoints(endpoints =>
{
    endpoints.MapDefaultControllerRoute();
});
app.Run();
