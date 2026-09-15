import { createContext, useContext } from 'react'
import type { Carrinho } from '../../api/tipos'

export const chaveCarrinho = (carrinhoId: string | null) => ['carrinho', carrinhoId] as const

export type CarrinhoContextoValor = {
  carrinhoId: string | null
  /** Devolve o id do carrinho atual, criando um carrinho na API se ainda não existir. */
  garantirCarrinho: () => Promise<string>
  /** Registra o carrinho devolvido pela API como o estado atual. */
  definirCarrinho: (carrinho: Carrinho) => void
  /** Esquece o carrinho atual (ex.: após "Nova compra"); o próximo será criado sob demanda. */
  esquecerCarrinho: () => void
}

export const CarrinhoContexto = createContext<CarrinhoContextoValor | null>(null)

export function useCarrinhoContexto(): CarrinhoContextoValor {
  const contexto = useContext(CarrinhoContexto)
  if (!contexto) {
    throw new Error('useCarrinhoContexto precisa estar dentro de <CarrinhoProvider>.')
  }
  return contexto
}
