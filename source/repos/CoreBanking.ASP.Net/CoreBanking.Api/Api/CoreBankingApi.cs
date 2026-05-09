




using CoreBanking.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

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

        private static async Task Transfer(Guid id)
        {
            throw new NotImplementedException();
        }

        private static async Task WithDraw(Guid id)
        {
            throw new NotImplementedException();
        }

        private static async Task Deposit(Guid id)
        {
            throw new NotImplementedException();
        }

        private static async Task GetAccounts(
            [AsParameters] CoreBankingService service,
            [AsParameters] PaginationRequest pagination)
        {
            throw new NotImplementedException();
        }

        private static async Task<Results<Ok<Customer>, BadRequest<string>>> CreateCustomer(
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

        private static async Task CreateAccount(HttpContext context)
        {
            throw new NotImplementedException();
        }
   
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
    }
}
