using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreBanking.Infrastructure.Data
{//class này dùng đẻ tạo một instance của CoreBankingDbContext tại thời gian thiết kế, thường được sử dụng trong quá trình phát triển để chạy các lệnh như migrations hoặc update-database mà không cần phải khởi động ứng dụng chính.
    public class CoreBankingContextFactory : IDesignTimeDbContextFactory<CoreBankingDbContext>
    {
        public CoreBankingDbContext CreateDbContext(string[] args)
        {
            var  optionsBuilder = new DbContextOptionsBuilder<CoreBankingDbContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Database=corebanking;Username=postgres;Password=postgres"); ;
            return new CoreBankingDbContext(optionsBuilder.Options);
        }
    }
}
