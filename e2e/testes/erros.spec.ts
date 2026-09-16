import { expect, test } from '@playwright/test'
import { Loja } from '../suporte/loja.js'

const AMULETO = 'Amuleto Rúnico de Proteção' // única unidade em estoque no catálogo
const POCAO = 'Poção de Cura Menor'

test('um cupom inexistente é recusado sem derrubar o cupom que já estava aplicado', async ({ page }) => {
  const loja = new Loja(page)
  await loja.abrir()
  await loja.adicionar(POCAO)
  await loja.aplicarCupom('10OFF')
  await expect(loja.sacola.getByText(/Cupom 10OFF aplicado/)).toBeVisible()

  await loja.aplicarCupom('NAOEXISTE')

  await expect(loja.sacola.getByRole('alert')).toHaveText("O cupom 'NAOEXISTE' é inválido ou não existe.")
  await expect(loja.sacola.getByText(/Cupom 10OFF aplicado/)).toBeVisible()
})

test('a interface não deixa passar do estoque disponível', async ({ page }) => {
  const loja = new Loja(page)
  await loja.abrir()
  await expect(loja.mercadoria(AMULETO)).toContainText('Última unidade em estoque')

  await loja.adicionar(AMULETO)

  await expect(loja.linhaDaSacola(AMULETO)).toContainText('Todo o estoque disponível já está na sacola')
  await expect(loja.mercadoria(AMULETO).getByRole('button', { name: `Adicionar ${AMULETO} à sacola` })).toBeDisabled()
  await expect(loja.mercadoria(AMULETO)).toContainText('(1 na sacola)')
})

test('não dá para finalizar uma sacola vazia', async ({ page }) => {
  const loja = new Loja(page)
  await loja.abrir()

  await expect(loja.sacola.getByRole('button', { name: 'Finalizar compra' })).toBeDisabled()
})
