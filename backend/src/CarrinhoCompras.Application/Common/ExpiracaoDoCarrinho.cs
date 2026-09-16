using CarrinhoCompras.Domain.Carrinhos;
using CarrinhoCompras.Domain.Common;

namespace CarrinhoCompras.Application.Common;

internal static class ExpiracaoDoCarrinho
{
    /// <summary>
    /// Expira a sacola se o prazo venceu, <b>gravando</b> essa mudança, e então diz se ela ainda aceita
    /// alterações.
    /// <para>
    /// A gravação é o ponto: quem descobre o vencimento é justamente a operação que vai ser recusada logo
    /// em seguida. Sem o commit aqui, o <c>return</c> do erro descartaria a devolução das unidades — elas
    /// ficariam presas até a varredura em segundo plano passar, e a consulta seguinte ainda mostraria a
    /// sacola como aberta, com um prazo no passado.
    /// </para>
    /// </summary>
    public static async Task<Result> GarantirAlteravelAsync(
        this Carrinho carrinho, DateTimeOffset agora, IUnitOfWork unitOfWork, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(carrinho);
        ArgumentNullException.ThrowIfNull(unitOfWork);

        if (carrinho.ExpirarSeVencido(agora))
        {
            await unitOfWork.CommitAsync(cancellationToken);
        }

        return carrinho.VerificarSePodeSerAlterado(agora);
    }
}
