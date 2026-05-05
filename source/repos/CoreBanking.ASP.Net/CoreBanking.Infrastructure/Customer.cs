using System.Text.Json.Serialization;

namespace CoreBanking.Infrastructure
{
    public class Customer
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;//dùng default! để chỉ định rằng thuộc tính này sẽ được khởi tạo sau khi đối tượng được tạo ra, và nó sẽ không có giá trị mặc định nào. Điều này giúp tránh lỗi null reference khi truy cập thuộc tính này trước khi nó được gán giá trị.
       
        public string Address { get; set; } = default!;
        [JsonIgnore]
        public ICollection<Account> Accounts { get; set; } = [];//dòng này khởi tạo một bộ sưu tập rỗng của các đối tượng Account, giúp tránh lỗi null reference khi truy cập thuộc tính Accounts trước khi nó được gán giá trị.
        
    }
}
