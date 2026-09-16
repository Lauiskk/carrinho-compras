// Espelho dos contratos da API. Valores monetários chegam já calculados pelo back-end:
// o front apenas exibe, nunca soma nem aplica desconto.
//
// Estoque tem dois números: `quantidadeEstoque` são as peças físicas da loja e `quantidadeDisponivel` é o
// que ainda dá para levar — o resto está reservado em sacolas abertas, inclusive na sua.

export type Produto = {
  id: number
  descricaoProduto: string
  precoLiquido: number
  quantidadeEstoque: number
  quantidadeReservada: number
  quantidadeDisponivel: number
}

export type StatusCarrinho = 'Aberto' | 'Finalizado' | 'Expirado'

export type ItemCarrinho = {
  produtoId: number
  descricaoProduto: string
  precoLiquidoUnitario: number
  quantidadeEstoque: number
  /** Quantas unidades ainda dá para somar a este item. */
  quantidadeDisponivel: number
  quantidade: number
  precoItem: number
}

export type CupomAplicado = {
  codigoCupom: string
  percentualDesconto: number
}

export type Carrinho = {
  id: string
  status: StatusCarrinho
  itens: ItemCarrinho[]
  cupom: CupomAplicado | null
  subtotal: number
  desconto: number
  total: number
  criadoEm: string
  finalizadoEm: string | null
  /** Até quando as unidades da sacola ficam guardadas; nulo quando ela está vazia, finalizada ou expirada. */
  expiraEm: string | null
}
