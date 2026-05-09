using Asp.Versioning;
using CoreBanking.Infrastructure.Data;
using CoreBanking.ServiceDefaults;
using Microsoft.EntityFrameworkCore;

namespace CoreBanking.Api.Bootstraping
{
    public static class ApplicationServiceExtentions
    {
        public static IHostApplicationBuilder AddApplicationService(this IHostApplicationBuilder builder)
        {
            builder.AddServiceDefaults();//dùng laij exxtention service default cua Aspire để cấu hình các dịch vụ mặc định như service discovery, resilience, health checks, và OpenTelemetry.

            // Add services to the container.
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            builder.Services.AddApiVersioning(
                options =>
                {
                    options.ReportApiVersions = true;
                    options.ApiVersionReader = ApiVersionReader.Combine(//có thể kết hợp nhiều cách để đọc version, ví dụ: từ URL, từ header, hoặc từ query string
                        new UrlSegmentApiVersionReader(),//cái này là để đọc version từ URL, ví dụ: /api/v1/endpoint
                        new HeaderApiVersionReader("X-Version")//dọc version từ header, ví dụ: X-Version: 1.0
                        );

                });

            //dăng ký dbcontext với PostgreSQL, và chỉ định assembly chứa migration
            builder.AddNpgsqlDbContext<CoreBankingDbContext>("corebanking-db", configureDbContextOptions: dbContextOptionsBuilder =>
            {
                dbContextOptionsBuilder.UseNpgsql(npgsql => npgsql.MigrationsAssembly(typeof(CoreBankingDbContext).Assembly.GetName().Name));
                //dòng này cấu hình DbContext để sử dụng PostgreSQL làm cơ sở dữ liệu, và chỉ định rằng các migration sẽ được tìm thấy trong assembly chứa lớp CoreBankingDbContext. Điều này giúp đảm bảo rằng khi chạy migration, Entity Framework sẽ biết nơi để tìm các lớp migration đã được tạo ra để cập nhật cơ sở dữ liệu một cách chính xác.


            });
            return builder;
        }
    }
}
