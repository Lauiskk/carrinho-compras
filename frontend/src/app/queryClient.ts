import { QueryClient } from '@tanstack/react-query'

export function criarQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: { staleTime: 15_000 },
      // Alterações do carrinho não são repetidas automaticamente: o usuário vê o erro e decide.
      mutations: { retry: false },
    },
  })
}
