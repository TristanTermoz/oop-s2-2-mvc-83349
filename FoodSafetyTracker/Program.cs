using FoodSafetyTracker.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Context;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, services, cfg) =>
        cfg.ReadFrom.Configuration(ctx.Configuration)
           .ReadFrom.Services(services)
           .Enrich.FromLogContext());

    var conn = builder.Configuration.GetConnectionString("DefaultConnection")
               ?? "Data Source=tracker.db";
    builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseSqlServer(conn));
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();
    builder.Services
        .AddDefaultIdentity<IdentityUser>(o => { o.SignIn.RequireConfirmedAccount = false; })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>();

    builder.Services.AddControllersWithViews();

    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }
    else
    {
        app.UseDeveloperExceptionPage();
        app.UseMigrationsEndPoint();
    }

    app.UseSerilogRequestLogging();
    app.Use(async (ctx, next) =>
    {
        using (LogContext.PushProperty("UserName",
            ctx.User.Identity?.IsAuthenticated == true
                ? ctx.User.Identity.Name
                : "anonymous"))
        {
            await next();
        }
    });

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(name: "default",
        pattern: "{controller=Dashboard}/{action=Index}/{id?}");
    app.MapRazorPages();

    await SeedData.InitialiseAsync(app.Services);

    Log.Information("Application starting up");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start");
}
finally
{
    Log.CloseAndFlush();
}
