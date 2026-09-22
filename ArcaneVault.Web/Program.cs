// Name: LIM RUI YANG RYAN
// Admin No: 252206Z
// Tutorial Group: IT2508
using ArcaneVault.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------
// Authorisation policies
// ---------------------------------------------------------------
// "StaffOnly" backs the Category Management and Vault Insights areas. It matches on the
// role claim written into the authentication cookie at login, which itself comes from
// the RoleName the API returned - so the role is decided by the database, not the browser.
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("StaffOnly", policy => policy.RequireRole("Staff"));
});

// ---------------------------------------------------------------
// Razor Pages, with folder-level access rules
// ---------------------------------------------------------------
// Declaring the rules here (rather than scattering [Authorize] attributes across every
// PageModel) means a newly added page inside a protected folder is secure by default -
// it is impossible to forget the attribute.
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/CollectionItems");           // any signed-in user
    options.Conventions.AuthorizeFolder("/Categories", "StaffOnly");   // administrators only
    options.Conventions.AuthorizeFolder("/Insights", "StaffOnly");     // administrators only
})
    // With <Nullable>enable</Nullable>, MVC infers [Required] for every non-nullable
    // reference-type property. That silently breaks two legitimate cases here:
    //   - CollectionItemInputDto.UserName, which is set server-side from the signed-in
    //     account and is deliberately never posted by the browser.
    //   - CollectionItemInputDto.CategoryCodes, when a collector ticks no categories.
    // Both would fail validation with an error message that is never displayed. Properties
    // that genuinely are required still carry an explicit [Required] attribute, so turning
    // the inference off removes only the unwanted behaviour.
    .AddMvcOptions(options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);

// ---------------------------------------------------------------
// Cookie authentication
// ---------------------------------------------------------------
// The web app owns the user's session; the API owns credential verification. On a
// successful login the API returns the account's details and this app converts them into
// a signed, encrypted authentication cookie.
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

// ---------------------------------------------------------------
// Typed HttpClient for ArcaneVault.Api
// ---------------------------------------------------------------
// The base address is plain HTTP on localhost, deliberately: a server-to-server HTTPS
// call would have to validate the development certificate, which is a common source of
// setup failures. Nothing sensitive crosses a network here - both processes are local.
var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
    ?? throw new InvalidOperationException("Missing required configuration value 'ApiBaseUrl'.");

builder.Services.AddHttpClient<ApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

// ---------------------------------------------------------------
// HTTP pipeline
// ---------------------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// Order matters: authentication establishes WHO the user is, and must run before
// authorisation decides WHAT they are allowed to reach.
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
