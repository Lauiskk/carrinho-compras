import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render } from '@testing-library/react'
import type { ReactElement } from 'react'
import { CarrinhoProvider } from '../features/carrinho/CarrinhoProvider'

/** Renderiza com os mesmos provedores da aplicação, sem repetições automáticas de requisição. */
export function renderizar(elemento: ReactElement) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  })

  return render(
    <QueryClientProvider client={queryClient}>
      <CarrinhoProvider>{elemento}</CarrinhoProvider>
    </QueryClientProvider>,
  )
}
