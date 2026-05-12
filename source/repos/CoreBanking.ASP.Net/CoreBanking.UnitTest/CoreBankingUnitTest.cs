using CoreBanking.Api.Api;
using CoreBanking.Api.Service;
using CoreBanking.Infrastructure;
using CoreBanking.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace CoreBanking.UnitTest
{
    public class CoreBankingUnitTest
    {
        private readonly SqliteConnection _sqliteConnection;
        private readonly CoreBankingDbContext _context;
        private readonly CoreBankingService _service;
        //viết như này tối ưu hơn về hiệu năng khi test
        public CoreBankingUnitTest()
        {
            _sqliteConnection = new SqliteConnection("DataSource=:memory:");
            _sqliteConnection.Open();
            var options = new DbContextOptionsBuilder<CoreBankingDbContext>()
                .UseSqlite(_sqliteConnection)
                .Options;
            _context = new CoreBankingDbContext(options);
            _context.Database.EnsureCreated();
            _service = new CoreBankingService(NullLogger<CoreBankingService>.Instance, _context);
        }
        internal void Dispose()
        {
            _context.Dispose();
            _sqliteConnection.Dispose();
        }
        [Fact]
        public void Create_Customer_UnitTest()
        {
            ///Arrange
       
                //ACT
                var customer = new Customer
                {
                    Id = Guid.NewGuid(),
                    Name = "John Doe",
                    Address = "123 Main"

                };
                

                var result = CoreBankingApi.CreateCustomer(_service, customer);

                // ASsert
                
                var addCustomer = _context.Customers.FirstOrDefault(c => c.Id == customer.Id);
                Assert.NotNull(addCustomer);
                Assert.Equal(customer.Name, addCustomer.Name);
                Assert.Equal(customer.Address, addCustomer.Address);
                Assert.Equal(customer.Accounts.Count, addCustomer.Accounts.Count);

            
        }

        [Theory]
        [InlineData(1000)]
        [InlineData(9999)]
        [InlineData(5000)]
        public void Create_Customer_And_Deposit_UnitTest(decimal depositAmount)
        {
            ///Arrange
            //_sqliteConnection = new SqliteConnection("DataSource=:memory:");
            //_sqliteConnection.Open();
            //_dbContextOptions = new DbContextOptionsBuilder<CoreBankingDbContext>()
            //    .UseSqlite(_sqliteConnection)
            //    .Options;
            //using (var context = new CoreBankingDbContext(_dbContextOptions))
            //{
            //    context.Database.EnsureCreated();
            //    var services = new CoreBankingService(NullLogger<CoreBankingService>.Instance, context);
            //    //ACT
                var customer = new Customer
                {
                    Id = Guid.NewGuid(),
                    Name = "John Doe",
                    Address = "123 Main"

                };


                var resultCustomer = CoreBankingApi.CreateCustomer(_service, customer);

                // ASsert
               
                var addCustomer = _context.Customers.FirstOrDefault(c => c.Id == customer.Id);
                Assert.NotNull(addCustomer);
                var account = new Account
                {
                    CustomerId = addCustomer.Id,
                };
                var resultAccount = CoreBankingApi.CreateAccount(_service, account);
                var addAccount = _context.Accounts.FirstOrDefault(a => a.CustomerId == addCustomer.Id);
             
                Assert.NotNull(addAccount);
         
                var  resultDeposit = CoreBankingApi.Deposit(_service, addAccount.Number, new DepositionRequest { Amount = depositAmount });
                Assert.NotNull(resultDeposit);
                Assert.Equal(depositAmount, addAccount.Balance);
                var  transaction = _context.Transactions.Where(t => t.AccountId == addAccount.Id);
                Assert.NotNull(transaction);
                Assert.Equal(depositAmount, transaction.First().Amount);
                Assert.Equal(1, transaction.Count());
            //}
        }
    }

}
