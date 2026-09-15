// Espelho dos contratos da API. Valores monetários chegam já calculados pelo back-end:
// o front apenas exibe, nunca soma nem aplica desconto.

export type Produto = {
  id: number
  descricaoProduto: string
  precoLiquido: number
  quantidadeEstoque: number
}

export type StatusCarrinho = 'Aberto' | 'Finalizado'

export type ItemCarrinho = {
  produtoId: number
  descricaoProduto: string
  precoLiquidoUnitario: number
  quantidadeEstoque: number
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
}
