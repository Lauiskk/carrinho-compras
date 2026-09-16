import { expect, test } from '@playwright/test'
import { Loja, moeda } from '../suporte/loja.js'

test('a sacola sobrevive a um recarregamento da página', async ({ page }) => {
  const loja = new Loja(page)
  await loja.abrir()
  const mercadoria = await loja.escolherMercadoria(2)
  const nome = mercadoria.descricaoProduto
  const preco = await loja.precoDeCatalogo(nome)

  await loja.adicionar(nome, 2)
  // Confere antes de recarregar: sem isso, um recarregamento rápido cancelaria as chamadas em voo e a
  // falha apareceria depois, apontando para o lugar errado.
  await expect(loja.linhaDaSacola(nome)).toContainText(`2 × ${moeda(preco)}`)

  await loja.aplicarCupom('15OFF')
  await expect(loja.sacola.getByText(/Cupom 15OFF aplicado/)).toBeVisible()
  const numeroDoCarrinho = await loja.sacola.getByText(/Carrinho nº/).textContent()

  await page.reload()

  await expect(loja.sacola.getByText(/Carrinho nº/)).toHaveText(numeroDoCarrinho ?? '')
  await expect(loja.linhaDaSacola(nome)).toContainText(`2 × ${moeda(preco)}`)
  await expect(loja.sacola.getByText(/Cupom 15OFF aplicado/)).toBeVisible()
  await expect(loja.sacola.getByText(/O mercador guarda estas peças por/)).toBeVisible()
})

test('uma aba desatualizada mostra a recusa da API e se ressincroniza', async ({ context }) => {
  // A aba A é a que age; a aba B fica com a tela antiga do mesmo carrinho.
  const abaA = new Loja(await context.newPage())
  await abaA.abrir()

  const comum = await abaA.escolherMercadoria(1)
  await abaA.adicionar(comum.descricaoProduto)
  // O id do carrinho só vai para o navegador quando o POST volta; a aba B lê o armazenamento ao montar.
  await expect(abaA.linhaDaSacola(comum.descricaoProduto)).toBeVisible()

  // Uma mercadoria escassa para as duas abas disputarem.
  const disputada = await abaA.escolherMercadoriaEscassa(1)
  const nome = disputada.descricaoProduto

  const abaB = new Loja(await context.newPage())
  await abaB.abrir()
  await expect(abaB.linhaDaSacola(comum.descricaoProduto)).toBeVisible()
  const adicionarNaB = abaB.mercadoria(nome).getByRole('button', { name: `Adicionar ${nome} à sacola` })
  await expect(adicionarNaB).toBeEnabled()

  // A aba A leva tudo o que restava. A aba B não fica sabendo: a leitura dela ainda está fresca.
  await abaA.adicionar(nome, disputada.quantidadeDisponivel)
  await expect(abaA.linhaDaSacola(nome)).toBeVisible()

  // --- Estoque: a aba B tenta pegar o que já não está disponível --------------------------------
  await expect(adicionarNaB, 'a aba B precisa continuar desatualizada para o cenário existir').toBeEnabled({
    timeout: 2_000,
  })
  await adicionarNaB.click()

  await expect(abaB.mercadoria(nome).getByRole('alert')).toContainText(`Estoque insuficiente para '${nome}'`)
  // E a tela desatualizada se corrige sozinha com o estado real do servidor.
  await expect(abaB.linhaDaSacola(nome)).toBeVisible()

  // --- Carrinho finalizado: qualquer alteração é recusada com mensagem clara ---------------------
  // A mercadoria escassa volta para a vitrine antes do checkout: o cenário já provou o que queria, e
  // vendê-la esvaziaria a prateleira para as rodadas seguintes.
  await abaA.removerDaSacola(nome)
  await expect(abaA.linhaDaSacola(nome)).toHaveCount(0)

  await abaA.finalizar()
  await expect(abaA.sacola.getByText(/Compra finalizada em/)).toBeVisible()

  const adicionarComumNaB = abaB
    .mercadoria(comum.descricaoProduto)
    .getByRole('button', { name: `Adicionar ${comum.descricaoProduto} à sacola` })
  await expect(adicionarComumNaB, 'a aba B ainda não sabe que a compra foi finalizada').toBeEnabled({ timeout: 2_000 })
  await adicionarComumNaB.click()

  await expect(abaB.mercadoria(comum.descricaoProduto).getByRole('alert')).toContainText(
    'Este carrinho já foi finalizado e não pode mais ser alterado',
  )
  await expect(abaB.sacola.getByText(/Compra finalizada em/)).toBeVisible()
  await expect(abaB.prateleiras.getByText('Esta compra foi finalizada e não aceita novas mercadorias.')).toBeVisible()
})
