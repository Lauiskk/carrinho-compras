using FluentValidation;

namespace CarrinhoCompras.Application.Carrinhos.AdicionarItem;

public sealed class AdicionarItemRequestValidator : AbstractValidator<AdicionarItemRequest>
{
    public AdicionarItemRequestValidator()
    {
        RuleFor(request => request.ProdutoId)
            .GreaterThan(0)
            .WithMessage("Informe o id de um produto do catálogo (maior que zero).");

        RuleFor(request => request.Quantidade)
            .GreaterThan(0)
            .WithMessage(Mensagens.QuantidadeMaiorQueZero);
    }
}
