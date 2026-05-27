using InvestmentWebApp.Data;
using InvestmentWebApp.Hubs;
using InvestmentWebApp.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddSingleton<EmailService>();
var dbPath = Path.Combine(builder.Environment.ContentRootPath, "app.db");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

Console.WriteLine($"Using SQLite database at: {dbPath}");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("DatabaseMigration");
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    try
    {
        var pendingMigrations = dbContext.Database.GetPendingMigrations().ToList();

        if (pendingMigrations.Count > 0)
        {
            logger.LogInformation(
                "Applying {MigrationCount} pending database migrations.",
                pendingMigrations.Count);
        }

        dbContext.Database.Migrate();
        logger.LogInformation("Database migrations completed successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database migration failed during application startup.");
        throw;
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapHub<DashboardHub>("/dashboardHub");


app.Run();
