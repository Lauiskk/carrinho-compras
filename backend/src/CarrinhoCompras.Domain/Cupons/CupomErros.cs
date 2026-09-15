using CarrinhoCompras.Domain.Common;

namespace CarrinhoCompras.Domain.Cupons;

public static class CupomErros
{
    public static Error Invalido(string codigoCupom) => Error.BusinessRule(
        "cupom.invalido",
        $"O cupom '{codigoCupom}' é inválido ou não existe.");
}
