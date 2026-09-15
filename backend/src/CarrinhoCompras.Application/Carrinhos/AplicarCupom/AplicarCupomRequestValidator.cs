using CarrinhoCompras.Domain.Cupons;
using FluentValidation;

namespace CarrinhoCompras.Application.Carrinhos.AplicarCupom;

public sealed class AplicarCupomRequestValidator : AbstractValidator<AplicarCupomRequest>
{
    public AplicarCupomRequestValidator() =>
        RuleFor(request => request.CodigoCupom)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Informe o código do cupom.")
            .Must(codigo => codigo.Trim().Length <= Cupom.CodigoTamanhoMaximo)
            .WithMessage($"O código do cupom deve ter no máximo {Cupom.CodigoTamanhoMaximo} caracteres.");
}
