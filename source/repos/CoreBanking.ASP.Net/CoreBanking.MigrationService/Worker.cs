using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Threading;

namespace CoreBanking.MigrationService;

public class Worker(IHostApplicationLifetime hostApplicationLifetime,ILogger<Worker> logger) : BackgroundService
{
    
    protected override   Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //try
        //{
        //    using var scope = serviceProvider.CreateScope();
        //    var dbContext = scope.ServiceProvider.GetRequiredService<CoreBankingDbContext>();
            
        //    await RunMigrationAsync(dbContext, stoppingToken);
        //    await SeedDataAsync(dbContext, stoppingToken);
        //}
        //catch (Exception ex)
        //{
           
        //    throw;
        //}
        logger.LogInformation("Migration completed successfully.");
        hostApplicationLifetime.StopApplication();
        return Task.CompletedTask;
    }
    //private static async Task RunMigrationAsync(
    //    TicketContext dbContext, CancellationToken cancellationToken)
    //{
    //    var strategy = dbContext.Database.CreateExecutionStrategy();
    //    await strategy.ExecuteAsync(async () =>
    //    {
    //        // Run migration in a transaction to avoid partial migration if it fails.
    //        await dbContext.Database.MigrateAsync(cancellationToken);
    //    });
    //}

    //private static async Task SeedDataAsync(
    //    TicketContext dbContext, CancellationToken cancellationToken)
    //{
    //    SupportTicket firstTicket = new()
    //    {
    //        Title = "Test Ticket",
    //        Description = "Default ticket, please ignore!",
    //        Completed = true
    //    };

    //    var strategy = dbContext.Database.CreateExecutionStrategy();
    //    await strategy.ExecuteAsync(async () =>
    //    {
    //        // Seed the database
    //        await using var transaction = await dbContext.Database
    //            .BeginTransactionAsync(cancellationToken);

    //        await dbContext.Tickets.AddAsync(firstTicket, cancellationToken);
    //        await dbContext.SaveChangesAsync(cancellationToken);
    //        await transaction.CommitAsync(cancellationToken);
    //    });
    //}
}
