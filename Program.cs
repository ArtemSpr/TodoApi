using Azure.Core;
using Azure.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TodoApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.Configure<AzureAdOptions>(
    builder.Configuration.GetSection("AzureAd"));

// Реєстрація DbContext з Azure AD токеном для запуску
builder.Services.AddDbContext<TodoContext>((sp, options) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var azureAd = sp.GetRequiredService<IOptions<AzureAdOptions>>().Value;
    var connectionString = config.GetConnectionString("SqlServer");

    var credential = new ClientSecretCredential(
        azureAd.TenanId,
        azureAd.ClientId,
        azureAd.ClientSecret);

    var token = credential.GetToken(
        new TokenRequestContext(new[] { "https://database.windows.net/.default" }));

    var conn = new SqlConnection(connectionString)
    {
        AccessToken = token.Token
    };

    options.UseSqlServer(conn);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TodoContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseAuthentication();

app.MapControllers();

app.Run();
