using Microsoft.AspNetCore.Authentication;
using MiSAP.Models;
using UserPortal.Middleware;
using UserPortal.Reposiratory;
using UserPortal.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<UserPortal.Middleware.AuthenticationMiddleware>();


builder.Services.AddScoped<MenuRepo>();
builder.Services.AddScoped<IMenuService, MenuService>();


builder.Services.AddScoped<LoginRepo>();
builder.Services.AddScoped<ILoginService, LoginService>();

builder.Services.AddScoped<UserRegistrationRepo>();
builder.Services.AddScoped<IUserRegistrationService, UserRegistrationService>();


builder.Services.AddScoped<SearchPopUpRepo>();



builder.Services.AddScoped<HomeRepo>();
builder.Services.AddScoped<IHomeService, HomeService>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.Name = "MySessionCookie";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    //  options.Cookie.SameSite = SameSiteMode.None;
    // options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;


});

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
app.UseSession();

app.UseMiddleware<UserPortal.Middleware.AuthenticationMiddleware>();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();
