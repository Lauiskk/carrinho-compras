using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;

namespace CarrinhoCompras.Api.Filters;

/// <summary>
/// Executa o validador FluentValidation do corpo da requisição (quando existir) antes da action.
/// Os erros entram no ModelState e saem pela mesma resposta 400 dos erros de model binding.
/// </summary>
internal sealed class ValidacaoRequestFilter(IServiceProvider services, IOptions<ApiBehaviorOptions> apiBehavior) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var parametro in context.ActionDescriptor.Parameters)
        {
            if (parametro.BindingInfo?.BindingSource != BindingSource.Body
                || !context.ActionArguments.TryGetValue(parametro.Name, out var corpo)
                || corpo is null)
            {
                continue;
            }

            var tipoValidador = typeof(IValidator<>).MakeGenericType(corpo.GetType());
            if (services.GetService(tipoValidador) is not IValidator validador)
            {
                continue;
            }

            var resultado = await validador.ValidateAsync(new ValidationContext<object>(corpo), context.HttpContext.RequestAborted);
            foreach (var falha in resultado.Errors)
            {
                context.ModelState.AddModelError(JsonNamingPolicy.CamelCase.ConvertName(falha.PropertyName), falha.ErrorMessage);
            }
        }

        if (!context.ModelState.IsValid)
        {
            context.Result = apiBehavior.Value.InvalidModelStateResponseFactory(context);
            return;
        }

        await next();
    }
}
