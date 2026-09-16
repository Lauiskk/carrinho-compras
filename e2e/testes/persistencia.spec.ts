import { expect, test } from '@playwright/test'
import { Loja, moeda } from '../suporte/loja.js'

const POCAO = 'Poção de Cura Menor'
const AMULETO = 'Amuleto Rúnico de Proteção' // única unidade em estoque: dá para disputá-la entre abas

test('a sacola sobrevive a um recarregamento da página', async ({ page }) => {
  const loja = new Loja(page)
  await loja.abrir()
  const preco = await loja.precoDeCatalogo(POCAO)
  await loja.adicionar(POCAO, 2)
  await loja.aplicarCupom('15OFF')
  const numeroDoCarrinho = await loja.sacola.getByText(/Carrinho nº/).textContent()

  await page.reload()

  await expect(loja.sacola.getByText(/Carrinho nº/)).toHaveText(numeroDoCarrinho ?? '')
  await expect(loja.linhaDaSacola(POCAO)).toContainText(`2 × ${moeda(preco)}`)
  await expect(loja.sacola.getByText(/Cupom 15OFF aplicado/)).toBeVisible()
})

test('uma aba desatualizada mostra a recusa da API e se ressincroniza', async ({ context }) => {
  // A aba A é a que age; a aba B fica com a tela antiga do mesmo carrinho.
  const abaA = new Loja(await context.newPage())
  await abaA.abrir()
  await abaA.adicionar(POCAO)

  const abaB = new Loja(await context.newPage())
  await abaB.abrir()
  await expect(abaB.linhaDaSacola(POCAO)).toBeVisible()
  await expect(abaB.mercadoria(AMULETO).getByRole('button', { name: `Adicionar ${AMULETO} à sacola` })).toBeEnabled()

  // A aba A leva a última unidade do amuleto. A aba B não fica sabendo (não há refetch em 15s).
  await abaA.adicionar(AMULETO)
  await expect(abaA.linhaDaSacola(AMULETO)).toBeVisible()

  // --- Estoque: a aba B tenta pegar o que já não está disponível --------------------------------
  await abaB.mercadoria(AMULETO).getByRole('button', { name: `Adicionar ${AMULETO} à sacola` }).click()

  await expect(abaB.mercadoria(AMULETO).getByRole('alert')).toContainText(`Estoque insuficiente para '${AMULETO}'`)
  // E a tela desatualizada se corrige sozinha com o estado real do servidor.
  await expect(abaB.linhaDaSacola(AMULETO)).toBeVisible()

  // --- Carrinho finalizado: qualquer alteração é recusada com mensagem clara ---------------------
  await abaA.finalizar()
  await expect(abaA.sacola.getByText(/Compra finalizada em/)).toBeVisible()

  await abaB.mercadoria(POCAO).getByRole('button', { name: `Adicionar ${POCAO} à sacola` }).click()

  await expect(abaB.mercadoria(POCAO).getByRole('alert')).toContainText(
    'Este carrinho já foi finalizado e não pode mais ser alterado',
  )
  await expect(abaB.sacola.getByText(/Compra finalizada em/)).toBeVisible()
  await expect(abaB.prateleiras.getByText('Esta compra foi finalizada e não aceita novas mercadorias.')).toBeVisible()
})
