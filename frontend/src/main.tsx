import '@fontsource-variable/grenze-gotisch'
import '@fontsource-variable/alegreya'
import '@fontsource-variable/alegreya/wght-italic.css'
import './styles/tokens.css'
import './styles/global.css'

import { QueryClientProvider } from '@tanstack/react-query'
import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { App } from './app/App'
import { criarQueryClient } from './app/queryClient'
import { CarrinhoProvider } from './features/carrinho/CarrinhoProvider'

const raiz = document.getElementById('root')
if (!raiz) {
  throw new Error('Elemento #root não encontrado.')
}

createRoot(raiz).render(
  <StrictMode>
    <QueryClientProvider client={criarQueryClient()}>
      <CarrinhoProvider>
        <App />
      </CarrinhoProvider>
    </QueryClientProvider>
  </StrictMode>,
)
