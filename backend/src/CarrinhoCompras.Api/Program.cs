using CarrinhoCompras.Api;
using CarrinhoCompras.Application;
using CarrinhoCompras.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure()
    .AddApi();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Configuration.GetValue("Documentacao:Habilitada", app.Environment.IsDevelopment()))
{
    app.MapOpenApi();
    app.UseSwaggerUI(opcoes =>
    {
        opcoes.SwaggerEndpoint("/openapi/v1.json", "Carrinho de Compras API v1");
        opcoes.DocumentTitle = "Carrinho de Compras API";
    });
}

app.MapHealthChecks("/health");
app.MapControllers();

if (app.Configuration.GetValue<bool>("Database:AplicarMigracoesNaInicializacao"))
{
    await app.Services.AplicarMigracoesAsync();
}

await app.RunAsync();
