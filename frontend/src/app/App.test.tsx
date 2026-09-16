import { screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { http, HttpResponse } from 'msw'
import type { Carrinho } from '../api/tipos'
import { carrinhoIdStorage } from '../features/carrinho/carrinhoIdStorage'
import { botas, carrinhoVazio, item, pocao, problema } from '../test/dados'
import { renderizar } from '../test/renderizar'
import { servidor } from '../test/servidor'
import { App } from './App'

const comProblema = (status: number, code: string, detail: string) =>
  HttpResponse.json(problema(status, code, detail), { status, headers: { 'Content-Type': 'application/problem+json' } })

/** API falsa com respostas prontas para o fluxo: os valores vêm "da API", o front só exibe. */
function simularApi() {
  let carrinho = carrinhoVazio()
  const base = `/api/carrinhos/${carrinho.id}`
  const atualizar = (novo: Partial<Carrinho>) => {
    carrinho = { ...carrinho, ...novo }
    return HttpResponse.json(carrinho)
  }

  servidor.use(
    http.get('/api/produtos', () => HttpResponse.json([pocao, botas])),
    http.post('/api/carrinhos', () => HttpResponse.json(carrinho, { status: 201 })),
    http.get(base, () => HttpResponse.json(carrinho)),
    http.post(`${base}/itens`, () =>
      atualizar({
        itens: [item(pocao, 2)],
        subtotal: 50,
        total: 50,
      }),
    ),
    http.put(`${base}/cupom`, async ({ request }) => {
      const { codigoCupom } = (await request.json()) as { codigoCupom: string }
      return codigoCupom.trim().toUpperCase() === '10OFF'
        ? atualizar({ cupom: { codigoCupom: '10OFF', percentualDesconto: 10 }, desconto: 5, total: 45 })
        : comProblema(422, 'cupom.invalido', `O cupom '${codigoCupom.toUpperCase()}' é inválido ou não existe.`)
    }),
    http.post(`${base}/finalizar`, () => atualizar({ status: 'Finalizado', finalizadoEm: '2026-09-15T21:30:00Z' })),
  )
}

describe('Fluxo completo da loja', () => {
  it('adiciona mercadoria, aplica cupom (com erro tratado) e finaliza a compra', async () => {
    simularApi()
    const usuario = userEvent.setup()
    renderizar(<App />)

    // Catálogo com preço e estoque
    expect(await screen.findByRole('heading', { name: pocao.descricaoProduto })).toBeInTheDocument()
    expect(screen.getByText('12 em estoque')).toBeInTheDocument()

    // Adicionar com quantidade escolhida
    await usuario.click(screen.getByRole('button', { name: `Aumentar quantidade de ${pocao.descricaoProduto}` }))
    await usuario.click(screen.getByRole('button', { name: `Adicionar ${pocao.descricaoProduto} à sacola` }))

    const sacola = screen.getByRole('complementary', { name: 'Sua sacola' })
    expect(await within(sacola).findByText(pocao.descricaoProduto)).toBeInTheDocument()
    expect(within(sacola).getByText('Total').nextElementSibling).toHaveTextContent('50,00 moedas de ouro')

    // Cupom inválido: mensagem da API junto do formulário
    await usuario.type(within(sacola).getByLabelText('Cupom de desconto'), 'naoexiste')
    await usuario.click(within(sacola).getByRole('button', { name: 'Aplicar' }))
    expect(await within(sacola).findByRole('alert')).toHaveTextContent("O cupom 'NAOEXISTE' é inválido ou não existe.")

    // Cupom válido
    await usuario.clear(within(sacola).getByLabelText('Cupom de desconto'))
    await usuario.type(within(sacola).getByLabelText('Cupom de desconto'), '10off')
    await usuario.click(within(sacola).getByRole('button', { name: 'Aplicar' }))
    expect(await within(sacola).findByText(/aplicado/)).toBeInTheDocument()
    expect(within(sacola).getByText('Desconto (10OFF, 10%)').nextElementSibling).toHaveTextContent('menos 5,00 moedas de ouro')
    expect(within(sacola).getByText('Total').nextElementSibling).toHaveTextContent('45,00 moedas de ouro')

    // Checkout
    await usuario.click(within(sacola).getByRole('button', { name: 'Finalizar compra' }))
    expect(await within(sacola).findByText(/Compra finalizada em/)).toBeInTheDocument()
    expect(within(sacola).queryByRole('button', { name: 'Finalizar compra' })).not.toBeInTheDocument()
    expect(within(sacola).queryByLabelText(/cupom/i)).not.toBeInTheDocument()
    expect(screen.getByRole('button', { name: `Adicionar ${pocao.descricaoProduto} à sacola` })).toBeDisabled()
    expect(screen.getByText('Esta compra foi finalizada e não aceita novas mercadorias.')).toBeInTheDocument()

    // Nova compra
    await usuario.click(within(sacola).getByRole('button', { name: 'Nova compra' }))
    expect(await within(sacola).findByText(/Sua sacola está vazia/)).toBeInTheDocument()
  })

  it('mostra junto do produto o erro de estoque e ressincroniza a sacola com o servidor', async () => {
    // A tela carrega o carrinho vazio; depois, outra aba coloca todo o estoque das botas nele.
    let carrinhoNoServidor = carrinhoVazio()
    carrinhoIdStorage.gravar(carrinhoNoServidor.id)
    servidor.use(
      http.get('/api/produtos', () => HttpResponse.json([pocao, botas])),
      http.get('/api/carrinhos/:id', () => HttpResponse.json(carrinhoNoServidor)),
      http.post('/api/carrinhos/:id/itens', () =>
        comProblema(422, 'produto.estoque_insuficiente', "Estoque insuficiente para 'Botas de Passos Silenciosos'."),
      ),
    )
    const usuario = userEvent.setup()
    renderizar(<App />)
    const sacola = screen.getByRole('complementary', { name: 'Sua sacola' })
    expect(await within(sacola).findByText(/Sua sacola está vazia/)).toBeInTheDocument()

    carrinhoNoServidor = carrinhoVazio({
      itens: [item(botas, 2)],
      subtotal: 240,
      total: 240,
    })
    await usuario.click(screen.getByRole('button', { name: `Adicionar ${botas.descricaoProduto} à sacola` }))

    const produto = screen.getByRole('heading', { name: botas.descricaoProduto }).closest('li') as HTMLElement
    expect(await within(produto).findByRole('alert')).toHaveTextContent("Estoque insuficiente para 'Botas de Passos Silenciosos'.")
    expect(await within(sacola).findByText(botas.descricaoProduto)).toBeInTheDocument()
    expect(within(sacola).getByText('Total').nextElementSibling).toHaveTextContent('240,00 moedas de ouro')
  })

  it('explica em bom português quando a sacola guardada não existe mais', async () => {
    // O id guardado no navegador aponta para um carrinho que o servidor perdeu entre a leitura e o clique
    // (ex.: banco de desenvolvimento recriado). A pessoa precisa saber o que houve — não pode falhar calada,
    // nem mostrar o id técnico do carrinho.
    const carrinho = carrinhoVazio()
    carrinhoIdStorage.gravar(carrinho.id)
    servidor.use(
      http.get('/api/produtos', () => HttpResponse.json([pocao, botas])),
      http.get(`/api/carrinhos/${carrinho.id}`, () => HttpResponse.json(carrinho)),
      http.post(`/api/carrinhos/${carrinho.id}/itens`, () =>
        comProblema(404, 'carrinho.nao_encontrado', `Carrinho '${carrinho.id}' não encontrado.`),
      ),
    )
    const usuario = userEvent.setup()
    renderizar(<App />)

    await usuario.click(await screen.findByRole('button', { name: `Adicionar ${pocao.descricaoProduto} à sacola` }))

    const produto = screen.getByRole('heading', { name: pocao.descricaoProduto }).closest('li') as HTMLElement
    const aviso = await within(produto).findByRole('alert')
    expect(aviso).toHaveTextContent('Sua sacola anterior não existe mais.')
    expect(aviso).not.toHaveTextContent(carrinho.id)
    await waitFor(() => expect(carrinhoIdStorage.ler()).toBeNull())
  })

  it('mostra a sacola recolhida quando a reserva expira', async () => {
    const expirado = carrinhoVazio({
      status: 'Expirado',
      itens: [item(pocao, 2)],
      subtotal: 50,
      total: 50,
      expiraEm: null,
    })
    carrinhoIdStorage.gravar(expirado.id)
    servidor.use(
      http.get('/api/produtos', () => HttpResponse.json([pocao, botas])),
      http.get(`/api/carrinhos/${expirado.id}`, () => HttpResponse.json(expirado)),
    )
    renderizar(<App />)

    const sacola = screen.getByRole('complementary', { name: 'Sua sacola' })
    expect(await within(sacola).findByText(/O mercador recolheu estas peças/)).toBeInTheDocument()
    // Nada de alterar: nem cupom, nem quantidade, nem checkout.
    expect(within(sacola).queryByRole('textbox')).not.toBeInTheDocument()
    expect(within(sacola).queryByRole('button', { name: 'Finalizar compra' })).not.toBeInTheDocument()
    expect(within(sacola).queryByRole('button', { name: `Remover ${pocao.descricaoProduto} da sacola` })).not.toBeInTheDocument()
    // Na prateleira, não no anúncio para leitor de tela (que repete o mesmo texto).
    const prateleiras = screen.getByRole('region', { name: 'Mercadorias' })
    expect(within(prateleiras).getByText('Sua sacola expirou e as mercadorias voltaram para a loja.')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: `Adicionar ${pocao.descricaoProduto} à sacola` })).toBeDisabled()
  })

  it('avisa que as mercadorias estão sendo recolhidas quando o prazo passa', async () => {
    // Janela em que o relógio do cliente já passou do prazo, mas o servidor ainda não encerrou a sacola
    // (quem encerra é a varredura em segundo plano ou a próxima alteração).
    const vencido = carrinhoVazio({
      itens: [item(pocao, 1)],
      subtotal: 25,
      total: 25,
      expiraEm: new Date(Date.now() - 60_000).toISOString(),
    })
    carrinhoIdStorage.gravar(vencido.id)
    servidor.use(
      http.get('/api/produtos', () => HttpResponse.json([pocao, botas])),
      http.get(`/api/carrinhos/${vencido.id}`, () => HttpResponse.json(vencido)),
    )
    renderizar(<App />)

    const sacola = screen.getByRole('complementary', { name: 'Sua sacola' })
    expect(await within(sacola).findByText(/O mercador está recolhendo suas mercadorias/)).toBeInTheDocument()
    expect(within(sacola).queryByText(/O mercador guarda estas peças/)).not.toBeInTheDocument()
  })

  it('avisa quando a API está fora do ar', async () => {
    servidor.use(http.get('/api/produtos', () => HttpResponse.error()))
    renderizar(<App />)

    expect(await screen.findByText('Não foi possível carregar as mercadorias.')).toBeInTheDocument()
    expect(screen.getByRole('alert')).toHaveTextContent('Sem conexão com a API.')
  })
})
