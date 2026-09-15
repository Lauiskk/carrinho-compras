using System.Text.Json;
using System.Text.Json.Serialization;
using CarrinhoCompras.Api.ErrorHandling;
using CarrinhoCompras.Api.Filters;
using Microsoft.OpenApi;

namespace CarrinhoCompras.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services
            .AddControllers(opcoes =>
            {
                opcoes.Filters.Add<ValidacaoRequestFilter>();
                // Sem isso, o MVC exigiria campos não anuláveis com mensagens próprias (em inglês),
                // antes do FluentValidation. As regras de obrigatoriedade ficam todas nos validadores.
                opcoes.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
                ProblemDetailsPadrao.TraduzirMensagensDeBinding(opcoes.ModelBindingMessageProvider);
            })
            .ConfigureApiBehaviorOptions(opcoes => opcoes.InvalidModelStateResponseFactory = RespostaRequisicaoInvalida.Criar)
            .AddJsonOptions(opcoes =>
            {
                ConfigurarJson(opcoes.JsonSerializerOptions);
                // Não expõe mensagens internas do desserializador (nomes de tipos .NET) para o cliente.
                opcoes.AllowInputFormatterExceptionMessages = false;
            });

        // O gerador de OpenAPI e o escritor de ProblemDetails usam estas opções (e não as do MVC).
        services.ConfigureHttpJsonOptions(opcoes => ConfigurarJson(opcoes.SerializerOptions));

        services.AddProblemDetails(opcoes => opcoes.CustomizeProblemDetails = ProblemDetailsPadrao.Aplicar);
        services.AddExceptionHandler<TratadorExcecoesGlobal>();

        services.AddOpenApi(opcoes =>
        {
            opcoes.AddDocumentTransformer((documento, _, _) =>
            {
                documento.Info = new OpenApiInfo
                {
                    Title = "Carrinho de Compras API",
                    Version = "v1",
                    Description =
                        "API REST de carrinho de compras: catálogo de produtos com estoque, itens, cupom de desconto, " +
                        "cálculo de subtotal/desconto/total e checkout. Todos os erros seguem o formato ProblemDetails " +
                        "(RFC 9457) com um campo `code` estável e mensagem em português.",
                };
                return Task.CompletedTask;
            });

            // Valores monetários são decimal no contrato (o padrão do gerador os descreveria como double).
            opcoes.AddSchemaTransformer((schema, contexto, _) =>
            {
                if (Nullable.GetUnderlyingType(contexto.JsonTypeInfo.Type) == typeof(decimal) || contexto.JsonTypeInfo.Type == typeof(decimal))
                {
                    schema.Format = "decimal";
                }

                return Task.CompletedTask;
            });
        });

        return services;
    }

    private static void ConfigurarJson(JsonSerializerOptions json)
    {
        json.Converters.Add(new JsonStringEnumConverter());
        // Tipos são parte do contrato: "quantidade": "2" (texto) é rejeitado.
        json.NumberHandling = JsonNumberHandling.Strict;
    }
}
