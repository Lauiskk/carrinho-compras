import { http } from './http'
import type { Carrinho } from './tipos'

export const carrinhosApi = {
  criar: () => http<Carrinho>('POST', '/carrinhos'),

  obter: (carrinhoId: string, signal?: AbortSignal) =>
    http<Carrinho>('GET', `/carrinhos/${carrinhoId}`, undefined, signal),

  adicionarItem: (carrinhoId: string, produtoId: number, quantidade: number) =>
    http<Carrinho>('POST', `/carrinhos/${carrinhoId}/itens`, { produtoId, quantidade }),

  alterarQuantidade: (carrinhoId: string, produtoId: number, quantidade: number) =>
    http<Carrinho>('PUT', `/carrinhos/${carrinhoId}/itens/${produtoId}`, { quantidade }),

  removerItem: (carrinhoId: string, produtoId: number) =>
    http<Carrinho>('DELETE', `/carrinhos/${carrinhoId}/itens/${produtoId}`),

  aplicarCupom: (carrinhoId: string, codigoCupom: string) =>
    http<Carrinho>('PUT', `/carrinhos/${carrinhoId}/cupom`, { codigoCupom }),

  removerCupom: (carrinhoId: string) => http<Carrinho>('DELETE', `/carrinhos/${carrinhoId}/cupom`),

  finalizar: (carrinhoId: string) => http<Carrinho>('POST', `/carrinhos/${carrinhoId}/finalizar`),
}
