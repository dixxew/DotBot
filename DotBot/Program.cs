using AspNet.Security.OAuth.Vkontakte;
using DotBot.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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




builder.Services.AddControllers();
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



builder.Services.AddCors();


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
app.UseCors(builder => builder.AllowAnyOrigin());


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.UseEndpoints(endpoints =>
{
    endpoints.MapDefaultControllerRoute();
});
app.Run();
