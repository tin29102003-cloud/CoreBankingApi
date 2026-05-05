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
            optionsBuilder.UseNpgsql("Host=localhost;Database=corebanking;Trusted_Connection=True;MultipleActiveResultSets=true");//sử dụng PostgreSQL làm provider, bạn có thể thay đổi chuỗi kết nối này để phù hợp với cơ sở dữ liệu của bạn
            return new CoreBankingDbContext(optionsBuilder.Options);
        }
    }
}
