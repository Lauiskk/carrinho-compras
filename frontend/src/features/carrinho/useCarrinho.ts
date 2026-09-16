import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect } from 'react'
import { carrinhosApi } from '../../api/carrinhosApi'
import { ApiError, CODIGO_SEM_CONEXAO } from '../../api/http'
import type { Carrinho } from '../../api/tipos'
import { chaveCarrinho, useCarrinhoContexto } from './carrinhoContexto'

/** Carrinho atual (ou null, se ainda não houver um). */
export function useCarrinho() {
  const { carrinhoId, esquecerCarrinho } = useCarrinhoContexto()

  const consulta = useQuery({
    queryKey: chaveCarrinho(carrinhoId),
    queryFn: ({ signal }) => carrinhosApi.obter(carrinhoId as string, signal),
    enabled: carrinhoId !== null,
    retry: (tentativas, erro) => !(erro instanceof ApiError && erro.status === 404) && tentativas < 2,
  })

  // O carrinho guardado no navegador não existe mais (ex.: banco recriado): começa do zero.
  useEffect(() => {
    if (consulta.error instanceof ApiError && consulta.error.code === 'carrinho.nao_encontrado') {
      esquecerCarrinho()
    }
  }, [consulta.error, esquecerCarrinho])

  return {
    carrinho: carrinhoId === null ? null : (consulta.data ?? null),
    carregando: carrinhoId !== null && consulta.isPending,
    erro: consulta.error,
    recarregar: consulta.refetch,
  }
}

/**
 * Base das alterações do carrinho. Todas compartilham o mesmo "scope": executam uma de cada vez,
 * na ordem dos cliques, e a resposta da API (o carrinho recalculado) substitui o estado local.
 */
function useAlteracaoDoCarrinho<TVariaveis>(executar: (carrinhoId: string, variaveis: TVariaveis) => Promise<Carrinho>) {
  const queryClient = useQueryClient()
  const { garantirCarrinho, definirCarrinho, esquecerCarrinho } = useCarrinhoContexto()

  return useMutation({
    scope: { id: 'carrinho' },
    mutationFn: async (variaveis: TVariaveis) => {
      const carrinhoId = await garantirCarrinho()
      // Uma leitura em andamento não pode sobrescrever o resultado desta alteração.
      await queryClient.cancelQueries({ queryKey: chaveCarrinho(carrinhoId) })
      return executar(carrinhoId, variaveis)
    },
    onSuccess: definirCarrinho,
    onError: (erro) => {
      if (!(erro instanceof ApiError) || erro.code === CODIGO_SEM_CONEXAO) {
        return
      }

      if (erro.code === 'carrinho.nao_encontrado') {
        // A sacola sumiu do servidor: recomeça do zero. O erro continua visível junto do botão clicado,
        // com um texto que explica o que houve — silenciar seria pior do que avisar.
        esquecerCarrinho()
        return
      }

      // Uma recusa da API (estoque, carrinho finalizado, concorrência...) pode significar que a tela está
      // desatualizada, por exemplo porque outra aba alterou o carrinho: recarrega o estado real do servidor.
      void queryClient.invalidateQueries({ queryKey: ['carrinho'] })
    },
  })
}

export const useAdicionarItem = () =>
  useAlteracaoDoCarrinho((carrinhoId, { produtoId, quantidade }: { produtoId: number; quantidade: number }) =>
    carrinhosApi.adicionarItem(carrinhoId, produtoId, quantidade),
  )

export const useAlterarQuantidade = () =>
  useAlteracaoDoCarrinho((carrinhoId, { produtoId, quantidade }: { produtoId: number; quantidade: number }) =>
    carrinhosApi.alterarQuantidade(carrinhoId, produtoId, quantidade),
  )

export const useRemoverItem = () =>
  useAlteracaoDoCarrinho((carrinhoId, produtoId: number) => carrinhosApi.removerItem(carrinhoId, produtoId))

export const useAplicarCupom = () =>
  useAlteracaoDoCarrinho((carrinhoId, codigoCupom: string) => carrinhosApi.aplicarCupom(carrinhoId, codigoCupom))

export const useRemoverCupom = () => useAlteracaoDoCarrinho<void>((carrinhoId) => carrinhosApi.removerCupom(carrinhoId))

export const useFinalizarCarrinho = () => useAlteracaoDoCarrinho<void>((carrinhoId) => carrinhosApi.finalizar(carrinhoId))
