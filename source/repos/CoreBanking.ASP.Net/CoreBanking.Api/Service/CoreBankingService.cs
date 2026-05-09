using CoreBanking.Infrastructure.Data;

namespace CoreBanking.Api.Service
{
    public class CoreBankingService(ILogger<CoreBankingService> logger, CoreBankingDbContext dbContext)
    {
        public CoreBankingDbContext DbContext { get; } = dbContext;
        public  ILogger<CoreBankingService> Logger => logger;
    }
}
