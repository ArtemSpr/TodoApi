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

builder.Services.AddDbContext<TodoContext>((sp, options) =>
{
    var config = sp.GetRequiredService<IConfiguration>();

    if (builder.Environment.IsDevelopment())
    {
        // Використовуємо SQL login для міграцій і локальної розробки
        options.UseSqlServer(config.GetConnectionString("DefaultConnection"));
    }
    else
    {
        // Використовуємо Azure AD токен на продакшн
        var azureAd = sp.GetRequiredService<IOptions<AzureAdOptions>>().Value;

        var credential = new ClientSecretCredential(
            azureAd.TenantId,
            azureAd.ClientId,
            azureAd.ClientSecret);

        var token = credential.GetToken(
            new TokenRequestContext(new[] { "https://database.windows.net/.default" }));

        var conn = new SqlConnection(config.GetConnectionString("DefaultConnectionAzureAD"))
        {
            AccessToken = token.Token
        };

        options.UseSqlServer(conn);
    }
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
app.MapControllers();
app.Run();
