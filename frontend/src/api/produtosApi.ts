import { http } from './http'
import type { Produto } from './tipos'

export const produtosApi = {
  listar: (signal?: AbortSignal) => http<Produto[]>('GET', '/produtos', undefined, signal),
}
