using FluentValidation;

namespace CarrinhoCompras.Application.Carrinhos.AlterarQuantidadeItem;

public sealed class AlterarQuantidadeItemRequestValidator : AbstractValidator<AlterarQuantidadeItemRequest>
{
    public AlterarQuantidadeItemRequestValidator() =>
        RuleFor(request => request.Quantidade)
            .GreaterThan(0)
            .WithMessage(Mensagens.QuantidadeMaiorQueZero);
}
