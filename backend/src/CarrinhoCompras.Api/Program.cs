using CarrinhoCompras.Application;
using CarrinhoCompras.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure();

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

if (app.Configuration.GetValue<bool>("Database:AplicarMigracoesNaInicializacao"))
{
    await app.Services.AplicarMigracoesAsync();
}

await app.RunAsync();
