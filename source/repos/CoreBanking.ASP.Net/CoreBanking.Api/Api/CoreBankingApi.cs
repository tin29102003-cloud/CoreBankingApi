





namespace CoreBanking.Api.Api
{
    public static class CoreBankingApi
    {
        public static IEndpointRouteBuilder MapCoreBankingApi(this IEndpointRouteBuilder builder)
        {
            var  api = builder.NewVersionedApi("CoreBankingApi");
            var  v1 = api.MapGroup("api/v{version:apiVersion}/corebanking").HasApiVersion(1, 0);//version 1.0
            v1.MapGet("/customer", GetCustomers);
            v1.MapPost("/customer", CreateCustomer);
            v1.MapGet("/accounts", GetAccounts);
            v1.MapPost("/accounts", CreateAccount);
            v1.MapPut("/accounts/{id:guid}/deposit", Deposit);
            v1.MapPut("/accounts/{id:guid}/withdraw", WithDraw);
            v1.MapPut("/accounts/{id:guid}/transfer", Transfer);
            return builder;
        }

        private static async Task<Results<Ok, BadRequest<string>, InternalServerError<string>>> Transfer(Guid id,
                [AsParameters] CoreBankingService service,
                TransferRequest transfer)
        {
            if (id == Guid.Empty)
            {
                service.Logger.LogError("Account id is required");
                return TypedResults.BadRequest("Account id is required");
            }
            if (string.IsNullOrEmpty(transfer.DestinationAccountNumber))
            {
                service.Logger.LogError("Destination account number is required");
                return TypedResults.BadRequest("Destination account number is required");
            }
            if (transfer.Amount <= 0)
            {
                service.Logger.LogError("Amount must be greater than 0");
                return TypedResults.BadRequest("Amount must be greater than 0");
            }
            var account = await service.DbContext.Accounts.FindAsync(id);
            if (account == null)
            {
                service.Logger.LogError("Account with id {AccountId} not found", id);
                return TypedResults.BadRequest($"Account with id {id} not found");
            }
            if (account.Balance < transfer.Amount)
            {
                service.Logger.LogError("Insufficient balance in account {AccountId}", id);
                return TypedResults.BadRequest("Insufficient balance");
            }
            var destinationAccount = await service.DbContext.Accounts.FirstOrDefaultAsync(a => a.Number == transfer.DestinationAccountNumber);
            if (destinationAccount == null)
            {
                service.Logger.LogError("Destination account with number {AccountNumber} not found", transfer.DestinationAccountNumber);
                return TypedResults.BadRequest($"Destination account with number {transfer.DestinationAccountNumber} not found");
            }
            if(destinationAccount.Number == account.Number)
            {
                service.Logger.LogError("Cannot transfer to the same account {AccountId}", id);
                return TypedResults.BadRequest("Cannot transfer to the same account");
            }
            account.Balance -= transfer.Amount;
            destinationAccount.Balance += transfer.Amount;
            try
            {
                var now  = DateTime.UtcNow;
                service.DbContext.Transactions.Add(new Transaction
                {
                    Id = Guid.CreateVersion7(),
                    AccountId = account.Id,
                    Amount = transfer.Amount,
                    Type = TransactionTypes.Withdraw,
                    DateUtc = now
                });
                service.DbContext.Transactions.Add(new Transaction
                {
                    Id = Guid.CreateVersion7(),
                    AccountId = destinationAccount.Id,
                    Amount = transfer.Amount,
                    Type = TransactionTypes.Deposit,
                    DateUtc = now
                });
                //service.DbContext.Accounts.Update(account);
                //service.DbContext.Accounts.Update(destinationAccount);
                await service.DbContext.SaveChangesAsync();
                return TypedResults.Ok();
            }
            catch (Exception ex)
            {
                service.Logger.LogError(ex, "An error whhile transferring");
                return TypedResults.InternalServerError("An error whhile transferring");
            }
        }
        public static async Task<Results<Ok<Account>, BadRequest<string>, InternalServerError<string>>> WithDraw(
              [AsParameters] CoreBankingService service,
            Guid id, WithdrawRequest withdraw)
        {
            if (id == Guid.Empty)
            {
                service.Logger.LogError("Account id is required");
                return TypedResults.BadRequest("Account id is required");
            }
            if (withdraw.Amount <= 0)
            {
                service.Logger.LogError("Amount must be greater than 0");
                return TypedResults.BadRequest("Amount must be greater than 0");
            }
            var account = await service.DbContext.Accounts.FindAsync(id);
            if (account == null)
            {
                service.Logger.LogError("Account with id {AccountId} not found", id);
                return TypedResults.BadRequest($"Account with id {id} not found");
            }
            account.Balance -= withdraw.Amount;
            if(account.Balance < 0)
            {
                service.Logger.LogError("Insufficient balance in account {AccountId}", id);
                return TypedResults.BadRequest("Insufficient balance");
            }
            try
            {
                service.DbContext.Transactions.Add(new Transaction
                {
                    Id = Guid.CreateVersion7(),
                    AccountId = account.Id,
                    Amount = withdraw.Amount,
                    Type = TransactionTypes.Withdraw,
                    DateUtc = DateTime.UtcNow
                });
                //service.DbContext.Accounts.Update(account);
                await service.DbContext.SaveChangesAsync();
                service.Logger.LogInformation("Withdraw {Amount} to account {AccountId}", withdraw.Amount, account.Id);
                return TypedResults.Ok(account);
            }
            catch (Exception ex)
            {
                service.Logger.LogError(ex, "An error whhile withdrawing");
                return TypedResults.InternalServerError("An error whhile withdrawing");
            }

        }

        internal static async Task<Results<Ok<Account>, BadRequest<string>, InternalServerError<string>>> Deposit(
              [AsParameters] CoreBankingService service,
            Guid id, DepositionRequest deposition)
        {
            if(id == Guid.Empty)
            {
                service.Logger.LogError("Account id is required");
                return TypedResults.BadRequest("Account id is required");
            }
            if(deposition.Amount <= 0)
            {
                service.Logger.LogError("Amount must be greater than 0");
                return TypedResults.BadRequest("Amount must be greater than 0");
            }
            var account = await service.DbContext.Accounts.FindAsync(id);   
            if(account == null)
            {
                service.Logger.LogError("Account with id {AccountId} not found", id);
                return TypedResults.BadRequest($"Account with id {id} not found");
            }
            account.Balance += deposition.Amount;
            try {
                service.DbContext.Transactions.Add(new Transaction
                {
                    Id = Guid.CreateVersion7(),
                    AccountId = account.Id,
                    Amount = deposition.Amount,
                    Type = TransactionTypes.Deposit,
                    DateUtc = DateTime.UtcNow
                });
                //service.DbContext.Accounts.Update(account);
                await service.DbContext.SaveChangesAsync();
                service.Logger.LogInformation("Deposited {Amount} to account {AccountId}", deposition.Amount, account.Id);
                return TypedResults.Ok(account);
            }
            catch(Exception ex)
            {
                service.Logger.LogError(ex, "An error whhile despositing");
                return TypedResults.InternalServerError("An error whhile despositing");
            }
            
        }

        #region customers
        private static async Task<Ok<PaginationResponse<Customer>>> GetCustomers(
           [AsParameters] CoreBankingService service,
           [AsParameters] PaginationRequest pagination)
        {     //attribute as paremater là một cách để chỉ định rằng các tham số của phương thức sẽ được lấy từ các tham số của yêu cầu HTTP, chẳng hạn như query string, route data hoặc form data. Điều này giúp cho việc xử lý các tham số trở nên dễ dàng hơn và giúp mã nguồn trở nên sạch sẽ hơn.
            return TypedResults.Ok(new PaginationResponse<Customer>(
                pagination.PageIndex,
                pagination.PageSize,
                await service.DbContext.Customers.LongCountAsync(),
                await service.DbContext.Customers
                .Skip(pagination.PageIndex * pagination.PageSize)
                .Take(pagination.PageSize)
                .OrderBy(c => c.Name)
                .ToListAsync()
            )
          );

        }
        public static async Task<Results<Ok<Customer>, BadRequest<string>>> CreateCustomer(
            [AsParameters] CoreBankingService service,
            Customer customer
            )
        {
            
            if (string.IsNullOrEmpty(customer.Name))
            {
                service.Logger.LogError("Customer name is required");
                return  TypedResults.BadRequest("Customer name is required");
            }
            customer.Address ??= "";
            if(customer.Id == Guid.Empty)
            {
                customer.Id = Guid.NewGuid();
            }
            service.DbContext.Customers.Add(customer);
            await service.DbContext.SaveChangesAsync();
            service.Logger.LogInformation("Created customer with id {CustomerId}", customer.Id);
            return TypedResults.Ok(customer);
        }
        #endregion
        #region accounts
        private static async Task<Ok<PaginationResponse<Account>>> GetAccounts(
           [AsParameters] CoreBankingService service,
           [AsParameters] PaginationRequest pagination,
            Guid? customerId = null)

        {
            IQueryable<Account> accounts = service.DbContext.Accounts;//đay là một Iqueryable<Account> đại diện cho tập hợp các tài khoản trong cơ sở dữ liệu. Nó cho phép bạn thực hiện các truy vấn LINQ để lọc, sắp xếp và phân trang dữ liệu một cách hiệu quả. Khi bạn gọi các phương thức như Skip, Take hoặc OrderBy trên accounts, chúng sẽ được chuyển đổi thành các câu lệnh SQL tương ứng khi truy vấn được thực thi, giúp tối ưu hóa hiệu suất và giảm thiểu lượng dữ liệu được tải về từ cơ sở dữ liệu.
            if (customerId.HasValue)
            {
                accounts = accounts.Where(a => a.CustomerId == customerId);
            }
            return TypedResults.Ok(new PaginationResponse<Account>(
                pagination.PageIndex,
                pagination.PageSize,
                await accounts.LongCountAsync(),
                await accounts
                .Skip(pagination.PageIndex * pagination.PageSize)
                .Take(pagination.PageSize)
                .OrderBy(c => c.Number)
                .ToListAsync()
            ));
        }

        internal static async Task<Results<Ok<Account>, BadRequest<string>>> CreateAccount(
             [AsParameters] CoreBankingService service,
            Account account
            )
        {
            if(account.CustomerId == Guid.Empty)
            {
                service.Logger.LogError("CustomerId is required");
                return TypedResults.BadRequest("CustomerId is required");
            }
            var customer = await service.DbContext.Customers.FindAsync(account.CustomerId);
            if(customer == null)
            {
                service.Logger.LogError("Customer with id {CustomerId} not found", account.CustomerId);
                return TypedResults.BadRequest($"Customer with id {account.CustomerId} not found");
            }
            account.Id = Guid.CreateVersion7();
            account.Balance = 0;
            account.Number = await GenerateAccountNumber(service);
            service.DbContext.Accounts.Add(account);
            await service.DbContext.SaveChangesAsync(); 
            service.Logger.LogInformation("Created account with id {AccountId} for customer {CustomerId}", account.Id, account.CustomerId);
            return TypedResults.Ok(account);    
        }
        #endregion
        private static async Task<string> GenerateAccountNumber(CoreBankingService service)
        {
           var random = new Random();
            var number = random.Next(10000000, 99999999).ToString();
            var exists = await service.DbContext.Accounts.AnyAsync(a => a.Number == number);
            if(exists)
            {
                return await GenerateAccountNumber(service);
            }
            return number;
        }
    }
}
public class DepositionRequest
{
    public decimal Amount { get; set; }
}
public class  WithdrawRequest
{
    public decimal Amount { get; set; }
}
public class TransferRequest
{
    public decimal Amount { get; set; }
    public string DestinationAccountNumber { get; set; } = default!;
}