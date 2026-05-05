

using CoreBanking.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);
//check 

builder.AddApplicationService();
var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapCoreBankingApi();

app.Run();


