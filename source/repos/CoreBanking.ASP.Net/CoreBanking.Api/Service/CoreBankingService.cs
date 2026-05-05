namespace CoreBanking.Api.Service
{
    public class CoreBankingService(ILogger<CoreBankingService> logger)
    {
        public  ILogger<CoreBankingService> Logger => logger;
    }
}
