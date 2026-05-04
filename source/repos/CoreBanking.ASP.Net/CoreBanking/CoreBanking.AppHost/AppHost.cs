
var builder = DistributedApplication.CreateBuilder(args);
//them 1 container của  postgers vào ứng dụng
var postgres = builder.AddPostgres("Postgres")//dòng nay sẽ thêm một container PostgreSQL vào ứng dụng phân tán với tên "Postgres", cho phép ứng dụng sử dụng cơ sở dữ liệu PostgreSQL để lưu trữ và quản lý dữ liệu.
        .WithImageTag("latest")//dòng này chỉ định rằng container PostgreSQL sẽ sử dụng phiên bản mới nhất của hình ảnh PostgreSQL từ Docker Hub.
        .WithVolume("corebanking-db", "/var/lib/postgresql")//dòng này là để cấu hình một volume có tên "corebanking-db" và gắn nó vào thư mục "/var/lib/postgresql/data" trong container PostgreSQL, giúp lưu trữ dữ liệu của cơ sở dữ liệu một cách bền vững.
        .WithLifetime(ContainerLifetime.Persistent)//dòng này chỉ định rằng container PostgreSQL sẽ có tuổi thọ bền vững, nghĩa là nó sẽ không bị xóa khi ứng dụng phân tán dừng lại hoặc khởi động lại, giúp đảm bảo rằng dữ liệu trong cơ sở dữ liệu PostgreSQL được giữ nguyên và không bị mất đi.  
        .WithPgAdmin(rbuilder =>//dong này sẽ thêm một container pgAdmin vào ứng dụng phân tán, cho phép quản lý cơ sở dữ liệu PostgreSQL thông qua giao diện web của pgAdmin. Hàm lambda được sử dụng để cấu hình container pgAdmin, trong đó rbuilder là đối tượng cấu hình cho container pgAdmin.
        {
            rbuilder.WithImageTag("latest");//ép buocj pgAdmin sử dụng phiên bản mới nhất của hình ảnh pgAdmin từ Docker Hub.
        });
var coreBankingDb = postgres.AddDatabase("corebanking-db","corebaking");

var miragationService = builder.AddProject<Projects.CoreBanking_MigrationService>("corebanking-migrationservice");//thằng này có nhiệm vụ khởi tạo db cho  chúng ta và có thể cập nhạt lại db nếu có gì mới 

builder.AddProject<Projects.CoreBanking_Api>("corebanking-api")//dòng  này sẽ thêm dự án CoreBanking.Api vào ứng dụng phân tán, cho phép nó được quản lý và chạy như một phần của hệ thống tổng thể.
    .WithReference(coreBankingDb)//tham chiếu đến cơ sở dữ liệu coreBankingDb, cho phép dự án CoreBanking.Api có thể truy cập và tương tác với cơ sở dữ liệu PostgreSQL đã được cấu hình trước đó.
    .WaitFor(postgres)//dòng này chỉ định rằng dự án CoreBanking.Api sẽ chờ cho đến khi container PostgreSQL được khởi động và sẵn sàng trước khi nó bắt đầu chạy, đảm bảo rằng cơ sở dữ liệu đã sẵn sàng để phục vụ các yêu cầu từ dự án CoreBanking.Api.
    .WaitForCompletion(miragationService);//chờ thằng migrationService hoàn thành trước khi dự án CoreBanking.Api bắt đầu chạy, đảm bảo rằng quá trình khởi tạo và cập nhật cơ sở dữ liệu đã hoàn tất trước khi dự án CoreBanking.Api bắt đầu phục vụ các yêu cầu từ người dùng hoặc các dịch vụ khác.
builder.Build().Run();
