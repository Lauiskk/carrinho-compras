using CarrinhoCompras.Domain.Carrinhos;

namespace CarrinhoCompras.Application.Common;

/// <summary>
/// Por quanto tempo a loja segura as mercadorias de uma sacola parada. É decisão comercial — uma loja de
/// ingressos segura por minutos, um supermercado nem segura — então vem da configuração
/// (<c>Carrinho:JanelaDeReserva</c>), com o padrão do domínio quando nada é informado.
/// </summary>
public sealed record PoliticaDeReserva(TimeSpan Janela)
{
    public static readonly PoliticaDeReserva Padrao = new(Carrinho.JanelaDeReservaPadrao);
}
