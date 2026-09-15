import { useQuery } from '@tanstack/react-query'
import { produtosApi } from '../../api/produtosApi'

export function useProdutos() {
  return useQuery({
    queryKey: ['produtos'],
    queryFn: ({ signal }) => produtosApi.listar(signal),
  })
}
