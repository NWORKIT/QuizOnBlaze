using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using QuizOnBlaze.Components;
using QuizOnBlaze.Hubs;
using QuizOnBlaze.Services;


namespace QuizOnBlaze
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Paths for Services like Session Manager Service
            var basePath = Environment.GetEnvironmentVariable("HOME") ?? Directory.GetCurrentDirectory();
            var dataFolder = Path.Combine(basePath, "data");
            var sessionsFolder = Path.Combine(basePath, "data", "sessions");
            Directory.CreateDirectory(dataFolder);
            Directory.CreateDirectory(sessionsFolder);

            // Add services to the container.
            builder.Logging.AddConsole();
            builder.Logging.SetMinimumLevel(LogLevel.Debug);

            builder.Services.AddLogging();

            builder.Services.AddSingleton<SessionManager>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<SessionManager>>();
                return new SessionManager(sessionsFolder, logger);
            });

            // Authentication & Authorization
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/login"; // login page path
                    options.Cookie.Name = ".AspNetCore.Cookies";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SameSite = SameSiteMode.Lax; // prevents blocking in modern browsers
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // for localhost in HTTP, use None
                    options.ExpireTimeSpan = TimeSpan.FromHours(1);
                    options.SlidingExpiration = true;
                });


            builder.Services.AddAuthorization();

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddSingleton<LoginService>();

            builder.Services.AddSingleton(new ForbiddenWordsService(dataFolder));

            builder.Services.AddSingleton<AvatarService>();

            builder.Services.AddScoped<PlayerStateService>();

            builder.Services.AddScoped<AdminActionService>();

            builder.Services.AddScoped<AdminStateService>();

            // Razor Components service
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            // After AddRazorPages
            builder.Services.AddSignalR();

            // After AddRazorComponents
            builder.Services.AddScoped(sp =>
            {
                var navigation = sp.GetRequiredService<NavigationManager>();
                return new HttpClient { BaseAddress = new Uri(navigation.BaseUri) };
            });

            // Register API Controllers
            builder.Services.AddControllers();

            builder.Services.AddServerSideBlazor()
                .AddCircuitOptions(options => { options.DetailedErrors = true; });

            builder.Services.AddServerSideBlazor()
                .AddCircuitOptions(options =>
                {
                    options.DetailedErrors = true;
                    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(5);
                })
                .AddHubOptions(options =>
                {
                    options.ClientTimeoutInterval = TimeSpan.FromMinutes(2); // Time the server waits for the client's message before closing
                    options.HandshakeTimeout = TimeSpan.FromSeconds(30);     // Maximum time for initial handshake
                    options.KeepAliveInterval = TimeSpan.FromMinutes(1);     // Interval to send pings to the client
                });

            builder.Services.AddSingleton<HubConnectionPlayerService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();


            app.UseStatusCodePages(async context =>
            {
                var response = context.HttpContext.Response;

                if (response.StatusCode == 401)
                    response.Redirect("/unauthorized");
                else if (response.StatusCode == 403)
                    response.Redirect("/forbidden");
                else if (response.StatusCode == 500)
                    response.Redirect("/error");
            });

            // For routing endpoints and middlewares
            app.UseRouting();

            //  SignalR endpoints Hub after Routing 
            app.MapHub<QuizHub>("/quizhub");

            app.UseAntiforgery();

            app.UseAuthentication();
            app.UseAuthorization();

            // Map controller routes for API processing
            app.MapControllers();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
