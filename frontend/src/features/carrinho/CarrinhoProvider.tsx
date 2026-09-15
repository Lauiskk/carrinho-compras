import { useQueryClient } from '@tanstack/react-query'
import { useCallback, useMemo, useRef, useState, type ReactNode } from 'react'
import { carrinhosApi } from '../../api/carrinhosApi'
import type { Carrinho } from '../../api/tipos'
import { CarrinhoContexto, chaveCarrinho } from './carrinhoContexto'
import { carrinhoIdStorage } from './carrinhoIdStorage'

export function CarrinhoProvider({ children }: { children: ReactNode }) {
  const queryClient = useQueryClient()
  const [carrinhoId, setCarrinhoId] = useState<string | null>(() => carrinhoIdStorage.ler())

  // Refs leem sempre o valor mais recente: alterações enfileiradas não podem usar um id "velho".
  const carrinhoIdAtual = useRef(carrinhoId)
  const criacaoEmAndamento = useRef<Promise<string> | null>(null)

  const definirCarrinho = useCallback(
    (carrinho: Carrinho) => {
      queryClient.setQueryData(chaveCarrinho(carrinho.id), carrinho)
      if (carrinhoIdAtual.current !== carrinho.id) {
        carrinhoIdAtual.current = carrinho.id
        carrinhoIdStorage.gravar(carrinho.id)
        setCarrinhoId(carrinho.id)
      }
    },
    [queryClient],
  )

  const esquecerCarrinho = useCallback(() => {
    carrinhoIdAtual.current = null
    carrinhoIdStorage.limpar()
    setCarrinhoId(null)
  }, [])

  const garantirCarrinho = useCallback(async () => {
    if (carrinhoIdAtual.current) {
      return carrinhoIdAtual.current
    }

    // Vários cliques simultâneos criam um único carrinho.
    criacaoEmAndamento.current ??= carrinhosApi
      .criar()
      .then((carrinho) => {
        definirCarrinho(carrinho)
        return carrinho.id
      })
      .finally(() => {
        criacaoEmAndamento.current = null
      })

    return criacaoEmAndamento.current
  }, [definirCarrinho])

  const valor = useMemo(
    () => ({ carrinhoId, garantirCarrinho, definirCarrinho, esquecerCarrinho }),
    [carrinhoId, garantirCarrinho, definirCarrinho, esquecerCarrinho],
  )

  return <CarrinhoContexto.Provider value={valor}>{children}</CarrinhoContexto.Provider>
}
