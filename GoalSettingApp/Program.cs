using GoalSettingApp.Components;
using GoalSettingApp.Services;
using GoalSettingApp.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Supabase Database Connection
var url = Environment.GetEnvironmentVariable("SUPABASE_URL");
var key = Environment.GetEnvironmentVariable("SUPABASE_KEY");
var options = new Supabase.SupabaseOptions
{
    AutoConnectRealtime = true
};
var supabase = new Supabase.Client(url, key, options);
await supabase.InitializeAsync();

// Register Supabase client in DI container
builder.Services.AddSingleton(supabase);

// Register HttpContextAccessor (needed for cookie auth in components)
builder.Services.AddHttpContextAccessor();

// Add authentication/authorization services
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
    });
builder.Services.AddAuthorizationCore(); // Enables [Authorize] attribute
builder.Services.AddScoped<AuthenticationStateProvider, SupabaseAuthenticationStateProvider>(); // Registers custom provider
builder.Services.AddCascadingAuthenticationState(); // Makes auth state available everywhere in app

// Register HttpClient
builder.Services.AddHttpClient();

// Register GoalService as scoped
builder.Services.AddScoped<GoalService>();

// Register WeatherService
builder.Services.AddScoped<WeatherService>();

// Email Settings - configure via environment variables
var emailSettings = new EmailSettings
{
    SmtpHost = Environment.GetEnvironmentVariable("SMTP_HOST") ?? "smtp.gmail.com",
    SmtpPort = int.TryParse(Environment.GetEnvironmentVariable("SMTP_PORT"), out var port) ? port : 587,
    SmtpUser = Environment.GetEnvironmentVariable("SMTP_USER") ?? "",
    SmtpPass = Environment.GetEnvironmentVariable("SMTP_PASS") ?? "",
    FromEmail = Environment.GetEnvironmentVariable("FROM_EMAIL") ?? "",
    FromName = Environment.GetEnvironmentVariable("FROM_NAME") ?? "Goal Setting App"
};
builder.Services.AddSingleton(emailSettings);

// Register EmailService
builder.Services.AddScoped<EmailService>();

// Register TaskReminderService (Background Service)
builder.Services.AddHostedService<TaskReminderService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.MapStaticAssets();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
