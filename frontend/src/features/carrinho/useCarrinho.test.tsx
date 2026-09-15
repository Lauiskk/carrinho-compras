import { screen, waitFor } from '@testing-library/react'
import { http, HttpResponse } from 'msw'
import { carrinhoVazio, problema } from '../../test/dados'
import { renderizar } from '../../test/renderizar'
import { servidor } from '../../test/servidor'
import { carrinhoIdStorage } from './carrinhoIdStorage'
import { useCarrinho } from './useCarrinho'

function SacolaDeTeste() {
  const { carrinho, carregando } = useCarrinho()
  if (carregando) {
    return <p>carregando</p>
  }
  return <p>{carrinho ? `carrinho ${carrinho.id}` : 'sem carrinho'}</p>
}

describe('useCarrinho', () => {
  it('recupera o carrinho guardado no navegador', async () => {
    const carrinho = carrinhoVazio()
    carrinhoIdStorage.gravar(carrinho.id)
    servidor.use(http.get(`/api/carrinhos/${carrinho.id}`, () => HttpResponse.json(carrinho)))

    renderizar(<SacolaDeTeste />)

    expect(await screen.findByText(`carrinho ${carrinho.id}`)).toBeInTheDocument()
  })

  it('esquece o carrinho guardado quando a API diz que ele não existe mais', async () => {
    carrinhoIdStorage.gravar('carrinho-apagado')
    servidor.use(
      http.get('/api/carrinhos/carrinho-apagado', () =>
        HttpResponse.json(problema(404, 'carrinho.nao_encontrado', 'Carrinho não encontrado.'), {
          status: 404,
          headers: { 'Content-Type': 'application/problem+json' },
        }),
      ),
    )

    renderizar(<SacolaDeTeste />)

    expect(await screen.findByText('sem carrinho')).toBeInTheDocument()
    await waitFor(() => expect(carrinhoIdStorage.ler()).toBeNull())
  })
})
