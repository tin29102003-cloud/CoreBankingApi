using CoreBanking.Infrastructure;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace CoreBanking.IntergationTests.Tests
{
    public class IntegrationTest1
    {
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

        // Instructions:
        // 1. Add a project reference to the target AppHost project, e.g.:
        //
        //    <ItemGroup>
        //        <ProjectReference Include="../MyAspireApp.AppHost/MyAspireApp.AppHost.csproj" />
        //    </ItemGroup>
        //
        // 2. Uncomment the following example test and update 'Projects.MyAspireApp_AppHost' to match your AppHost project:
        //
        [Fact]
        public async Task GetWebResourceRootReturnsOkStatusCode()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.CoreBanking_AppHost>(cancellationToken);
            appHost.Services.AddLogging(logging =>
            {
                logging.SetMinimumLevel(LogLevel.Debug);
                // Override the logging filters from the app's configuration
                logging.AddFilter(appHost.Environment.ApplicationName, LogLevel.Debug);
                logging.AddFilter("Aspire.", LogLevel.Debug);
                // To output logs to the xUnit.net ITestOutputHelper, consider adding a package from https://www.nuget.org/packages?q=xunit+logging
            });
            appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
            {
                clientBuilder.AddStandardResilienceHandler();
            });

            await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
            await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

            // Act
            using var httpClient = app.CreateHttpClient("corebanking-api");
            await app.ResourceNotifications.WaitForResourceHealthyAsync("corebanking-api", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
            //start testing when the resource is healthy to avoid flaky tests due to startup time
            //arrange the test data and make the API call
            var customer1 = new Customer()
            {
                Id = Guid.NewGuid(),
                Name = "John Doe",
                Address = "123 Main",

            };
            var customer2 = new Customer()
            {
                Id = Guid.NewGuid(),
                Name = "Jane Doe",
                Address = "456 Main",
            };
            var response = await httpClient.PostAsJsonAsync("api/v1/corebanking/customer", customer1);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var response2 = await httpClient.PostAsJsonAsync("api/v1/corebanking/customer", customer2);
            Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
            var account1 = new Account()
            {
               
                CustomerId = customer1.Id
            };
            var account2 = new Account()
            {
                  CustomerId = customer2.Id
            };
            //act
            response = await httpClient.PostAsJsonAsync("api/v1/corebanking/accounts", account1);
            //asset
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            //act
            var getAccount1 = await response.Content.ReadFromJsonAsync<Account>();
            //assert
            Assert.NotNull(getAccount1);
            Assert.NotEmpty(getAccount1.Number);
            Assert.Equal(account1.CustomerId, getAccount1.CustomerId);
            Assert.Equal(0.00m, getAccount1.Balance);
            
            //act
            response2 = await httpClient.PostAsJsonAsync("api/v1/corebanking/accounts", account2);
            //assert
            Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
            var getAccount2 = await response2.Content.ReadFromJsonAsync<Account>();
            //assert
            Assert.NotNull(getAccount2);
           
            Assert.Equal(account2.CustomerId, getAccount2.CustomerId);
            Assert.Equal(0.00m, getAccount2.Balance);
            //act   
           // var getResponse1 = await httpClient.GetAsync($"api/v1/corebanking/accounts/{getAccount1.Id}");
           // //asset
           //Assert.Equal(HttpStatusCode.OK, getResponse1.StatusCode);
           // //act
           // getAccount2 = await getResponse1.Content.ReadFromJsonAsync<Account>();
           // //assert
           // Assert.NotNull(getAccount2);
            
           // Assert.Equal(account2.CustomerId, getAccount2.CustomerId);
           // Assert.Equal(0.00m, getAccount2.Balance);
           // var getResponse2 = await httpClient.GetAsync($"api/v1/corebanking/accounts/{getAccount2.Id}");
           // //assert
           // Assert.Equal(HttpStatusCode.OK, getResponse2.StatusCode);
           // //act
           // getAccount2 = await getResponse2.Content.ReadFromJsonAsync<Account>();
           // //assert
           // Assert.NotNull(getAccount2);
          
           // Assert.Equal(account2.CustomerId, getAccount2.CustomerId);
           // Assert.Equal(0.00m, getAccount2.Balance);
            //act
            response = await httpClient.PutAsJsonAsync($"api/v1/corebanking/accounts/{getAccount1.Number}/deposit", new DepositionRequest()
            {
                Amount = 50000.00m
            });
            //assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            //act
            response = await httpClient.PutAsJsonAsync($"api/v1/corebanking/accounts/{getAccount2.Number}/withdraw", new WithdrawRequest()
            {
                Amount = 50000.00m
            });
            //assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            //act
            response = await httpClient.PutAsJsonAsync($"api/v1/corebanking/accounts/{getAccount2.Number}/deposit", new WithdrawRequest()
            {
                Amount = 50000.00m
            });
            //assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            //act
            response = await httpClient.PutAsJsonAsync($"api/v1/corebanking/accounts/{getAccount1.Number}/withdraw", new WithdrawRequest()
            {
                Amount = 20000.00m
            });
            //assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            //act
            response = await httpClient.PutAsJsonAsync($"api/v1/corebanking/accounts/{getAccount1.Number}/transfer", new TransferRequest()
            {
                Amount = 100000.00m,
                DestinationAccountNumber = getAccount2.Number
            });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            //act
            response = await httpClient.PutAsJsonAsync($"api/v1/corebanking/accounts/{getAccount1.Number}/transfer", new TransferRequest()
            {
                Amount = 30000.00m,
                DestinationAccountNumber = getAccount2.Number
            });
            //assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            //act
             var getResponse1 = await httpClient.GetAsync($"api/v1/corebanking/accounts/{getAccount1.Id}");
            //asset
            Assert.Equal(HttpStatusCode.OK, getResponse1.StatusCode);
            //act
             getAccount1 = await getResponse1.Content.ReadFromJsonAsync<Account>();
            //assert
            Assert.NotNull(getAccount1);
            //Assert.Equal(account1.Id, getAccount1.Id);
            Assert.Equal(account1.CustomerId, getAccount1.CustomerId);
            Assert.Equal(0.00m, getAccount1.Balance);
            //act
            var getResponse2 = await httpClient.GetAsync($"api/v1/corebanking/accounts/{getAccount2.Id}");
            //asset
            Assert.Equal(HttpStatusCode.OK, getResponse1.StatusCode);
            //act
            getAccount2 = await getResponse2.Content.ReadFromJsonAsync<Account>();
            //assert
            Assert.NotNull(getAccount2);
            //Assert.Equal(account2.Id, getAccount2.Id);
            Assert.Equal(account2.CustomerId, getAccount2.CustomerId);
            Assert.Equal(80000.00m, getAccount2.Balance);


        }
    }
}
