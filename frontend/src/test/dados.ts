import type { Carrinho, ItemCarrinho, Produto } from '../api/tipos'

/** Produto do catálogo, por padrão sem nada reservado (tudo disponível). */
function produto(dados: Omit<Produto, 'quantidadeReservada' | 'quantidadeDisponivel'> & Partial<Produto>): Produto {
  return { quantidadeReservada: 0, quantidadeDisponivel: dados.quantidadeEstoque, ...dados }
}

export const pocao = produto({ id: 1, descricaoProduto: 'Poção de Cura Menor', precoLiquido: 25, quantidadeEstoque: 12 })
export const botas = produto({ id: 8, descricaoProduto: 'Botas de Passos Silenciosos', precoLiquido: 120, quantidadeEstoque: 2 })

/** Linha de sacola, por padrão com o restante do estoque ainda disponível. */
export function item(base: Produto, quantidade: number, disponivel?: number): ItemCarrinho {
  return {
    produtoId: base.id,
    descricaoProduto: base.descricaoProduto,
    precoLiquidoUnitario: base.precoLiquido,
    quantidadeEstoque: base.quantidadeEstoque,
    quantidadeDisponivel: disponivel ?? base.quantidadeEstoque - quantidade,
    quantidade,
    precoItem: base.precoLiquido * quantidade,
  }
}

export function carrinhoVazio(sobrescrever: Partial<Carrinho> = {}): Carrinho {
  return {
    id: '0199a3c4-0000-7000-8000-000000000001',
    status: 'Aberto',
    itens: [],
    cupom: null,
    subtotal: 0,
    desconto: 0,
    total: 0,
    criadoEm: '2026-09-15T20:00:00Z',
    finalizadoEm: null,
    expiraEm: null,
    ...sobrescrever,
  }
}

export function problema(status: number, code: string, detail: string, errors?: Record<string, string[]>) {
  return {
    type: 'https://tools.ietf.org/html/rfc9110',
    title: 'Erro',
    status,
    detail,
    instance: '/api/teste',
    traceId: '00-teste-00',
    code,
    ...(errors ? { errors } : {}),
  }
}
