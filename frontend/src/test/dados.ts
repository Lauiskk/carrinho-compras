import type { Carrinho, Produto } from '../api/tipos'

export const pocao: Produto = { id: 1, descricaoProduto: 'Poção de Cura Menor', precoLiquido: 25, quantidadeEstoque: 12 }
export const botas: Produto = { id: 8, descricaoProduto: 'Botas de Passos Silenciosos', precoLiquido: 120, quantidadeEstoque: 2 }

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
