using CoreBanking.Infrastructure.Data;
using CoreBanking.MigrationService;
using CoreBanking.ServiceDefaults;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddHostedService<Worker>();
builder.AddNpgsqlDbContext<CoreBankingDbContext>("corebanking-db", configureDbContextOptions: dbContextOptionsBuilder =>
{
    dbContextOptionsBuilder.UseNpgsql(npgsql => npgsql.MigrationsAssembly(typeof(CoreBankingDbContext).Assembly.GetName().Name));
    //dòng này cấu hình DbContext để sử dụng PostgreSQL làm cơ sở dữ liệu, và chỉ định rằng các migration sẽ được tìm thấy trong assembly chứa lớp CoreBankingDbContext. Điều này giúp đảm bảo rằng khi chạy migration, Entity Framework sẽ biết nơi để tìm các lớp migration đã được tạo ra để cập nhật cơ sở dữ liệu một cách chính xác.


});
//nó sẽ lấy name từ cấu hình của container PostgreSQL đã được thêm vào ứng dụng phân tán trước đó, giúp kết nối đến cơ sở dữ liệu PostgreSQL để thực hiện các hoạt động liên quan đến migration và seeding dữ liệu.

//lấy từ postgresql service result của container PostgreSQL đã được thêm vào ứng dụng phân tán trước đó, giúp kết nối đến cơ sở dữ liệu PostgreSQL để thực hiện các hoạt động liên quan đến migration và seeding dữ liệu.
var host = builder.Build();
host.Run();
