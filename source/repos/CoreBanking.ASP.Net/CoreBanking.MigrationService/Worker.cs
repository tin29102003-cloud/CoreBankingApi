using CoreBanking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Threading;

namespace CoreBanking.MigrationService;

public class Worker(IHostApplicationLifetime hostApplicationLifetime, IServiceProvider serviceProvider, ILogger<Worker> logger) : BackgroundService
{
    
    protected override async   Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            logger.LogInformation("Starting migration...");
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CoreBankingDbContext>();
            //thiếu hàm ensure để tạo database nếu chưa tồn tại, nếu không có hàm này thì khi chạy migration sẽ bị lỗi vì database chưa tồn tại
            var databaseExists = await dbContext.Database.CanConnectAsync(stoppingToken);

            if (!databaseExists)
            {
                logger.LogInformation("Database does not exist. MigrateAsync will create it.");
            }
            var allMigrations = dbContext.Database.GetMigrations(); // tất cả migrations trong assembly
            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync(); // chưa apply

            logger.LogInformation("Tất cả migrations: {All}", allMigrations.Count());
            logger.LogInformation("Pending migrations: {Pending}", pendingMigrations.Count());
            //foreach (var m in migrations)
            //{
            //    logger.LogInformation("Tìm thấy bản Migration: {Name}", m);
            //}
            logger.LogInformation("Ensuring database is created...");   
            await RunMigrationAsync(dbContext, stoppingToken);
            //await SeedDataAsync(dbContext, stoppingToken);
            logger.LogInformation("Migration and seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during migration.");
            throw;
        }
        logger.LogInformation("Migration completed successfully.");
        hostApplicationLifetime.StopApplication();
     
    }
    private static async Task RunMigrationAsync(//ham chay migration trong transaction de tranh viec migration bi fail mot phan
        CoreBankingDbContext dbContext, CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            // Run migration in a transaction to avoid partial migration if it fails.
            await dbContext.Database.MigrateAsync(cancellationToken);
        });
    }

    private static async Task SeedDataAsync(
        CoreBankingDbContext dbContext, CancellationToken cancellationToken)
    {
       

        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            // Seed the database
            await using var transaction = await dbContext.Database
                .BeginTransactionAsync(cancellationToken);

            //await dbContext.Tickets.AddAsync(firstTicket, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });
    }
}
