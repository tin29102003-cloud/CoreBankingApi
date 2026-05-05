using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace CoreBanking.Infrastructure
{
    public class Account
    {
        public Guid Id { get; set; }
        public string Number { get; set; } = default!;
        public decimal Balance { get; set; }
        public Guid CustomerId { get; set; }
        [JsonIgnore]//cái này sẽ giúp bỏ qua thuộc tính Customer khi thực hiện quá trình serialize hoặc deserialize đối tượng Account thành JSON, giúp tránh lỗi vòng lặp vô hạn khi có mối quan hệ giữa Account và Customer.
        public Customer Customer { get; set; } = default!;//các thuộc tính dữ thừa ko phù hợp  thì gán cho jsonignore để khi serialize hoặc deserialize đối tượng Account thành JSON, thuộc tính Customer sẽ bị bỏ qua, giúp tránh lỗi vòng lặp vô hạn khi có mối quan hệ giữa Account và Customer. 
        [JsonIgnore]
        public ICollection<Transaction> Transactions { get; set; } = [];

    }
}
