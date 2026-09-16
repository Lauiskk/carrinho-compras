import { expect, test } from '@playwright/test'
import { descontoEsperado, Loja, moeda } from '../suporte/loja.js'

const POCAO = 'Poção de Cura Menor'
const LANTERNA = 'Lanterna de Óleo'

test('percorre catálogo, sacola, cupom, totais e checkout', async ({ page }) => {
  const loja = new Loja(page)
  await loja.abrir()

  // --- Catálogo ---------------------------------------------------------------------------------
  await expect(loja.prateleiras.getByRole('listitem')).toHaveCount(10)
  await loja.conferirRotulosDoResumo()
  await expect(loja.sacola).toContainText('Sua sacola está vazia')

  // Os preços vêm do catálogo: daqui em diante o teste calcula os totais por conta própria
  // e compara com o que a API devolveu — o front não é a fonte da verdade dos valores.
  const precoPocao = await loja.precoDeCatalogo(POCAO)
  const precoLanterna = await loja.precoDeCatalogo(LANTERNA)
  expect(precoPocao).toBeGreaterThan(0)
  expect(precoLanterna).toBeGreaterThan(0)

  // --- Adicionar: produto novo entra com a quantidade escolhida (padrão 1) -----------------------
  await loja.adicionar(POCAO)
  await expect(loja.linhaDaSacola(POCAO)).toContainText(`1 × ${moeda(precoPocao)}`)
  await expect(loja.precoDoItem(POCAO)).toHaveText(`${moeda(precoPocao)} moedas de ouro`)
  await loja.conferirValores({ subtotal: precoPocao, desconto: 0, total: precoPocao })

  // --- Adicionar de novo: soma à quantidade que já estava lá -------------------------------------
  await loja.adicionar(POCAO, 3)
  await expect(loja.linhaDaSacola(POCAO)).toContainText(`4 × ${moeda(precoPocao)}`)
  await loja.conferirValores({ subtotal: precoPocao * 4, desconto: 0, total: precoPocao * 4 })

  // --- Alterar quantidade na sacola: substitui, para menos -----------------------------------------
  await loja.definirQuantidadeNaSacola(POCAO, 4, 2)
  await expect(loja.precoDoItem(POCAO)).toHaveText(`${moeda(precoPocao * 2)} moedas de ouro`)
  await loja.conferirValores({ subtotal: precoPocao * 2, desconto: 0, total: precoPocao * 2 })

  // --- Segundo produto --------------------------------------------------------------------------
  await loja.adicionar(LANTERNA)
  const subtotal = precoPocao * 2 + precoLanterna
  await loja.conferirValores({ subtotal, desconto: 0, total: subtotal })

  // --- Cupom: aplicar ---------------------------------------------------------------------------
  await loja.aplicarCupom('10off') // minúsculo de propósito: o código é normalizado
  await expect(loja.sacola.getByText(/Cupom 10OFF aplicado/)).toBeVisible()
  const desconto10 = descontoEsperado(subtotal, 10)
  await loja.conferirValores({ subtotal, desconto: desconto10, total: subtotal - desconto10 })

  // --- Cupom: trocar (só um ativo por vez) ------------------------------------------------------
  await loja.aplicarCupom('15OFF')
  await expect(loja.sacola.getByText(/Cupom 15OFF aplicado/)).toBeVisible()
  await expect(loja.sacola.getByText(/Cupom 10OFF aplicado/)).toBeHidden()
  const desconto15 = descontoEsperado(subtotal, 15)
  await loja.conferirValores({ subtotal, desconto: desconto15, total: subtotal - desconto15 })

  // --- Cupom: remover ---------------------------------------------------------------------------
  await loja.removerCupom()
  await expect(loja.sacola.getByText(/aplicado/)).toBeHidden()
  await loja.conferirValores({ subtotal, desconto: 0, total: subtotal })

  // --- Remover item: totais recalculam ----------------------------------------------------------
  await loja.removerDaSacola(LANTERNA)
  await expect(loja.linhaDaSacola(LANTERNA)).toHaveCount(0)
  await loja.conferirValores({ subtotal: precoPocao * 2, desconto: 0, total: precoPocao * 2 })

  // --- Checkout ---------------------------------------------------------------------------------
  await loja.aplicarCupom('10OFF')
  const subtotalFinal = precoPocao * 2
  const descontoFinal = descontoEsperado(subtotalFinal, 10)
  await loja.conferirValores({ subtotal: subtotalFinal, desconto: descontoFinal, total: subtotalFinal - descontoFinal })

  await loja.finalizar()
  await expect(loja.sacola.getByText(/Compra finalizada em/)).toBeVisible()
  await expect(loja.sacola.getByRole('button', { name: 'Finalizar compra' })).toHaveCount(0)

  // Carrinho finalizado não aceita mais alterações: nem itens, nem quantidade, nem cupom.
  await expect(loja.sacola.getByRole('textbox')).toHaveCount(0)
  await expect(loja.linhaDaSacola(POCAO).getByRole('button', { name: /Remover|quantidade/ })).toHaveCount(0)
  await expect(loja.prateleiras.getByText('Esta compra foi finalizada e não aceita novas mercadorias.')).toBeVisible()
  await expect(loja.mercadoria(POCAO).getByRole('button', { name: `Adicionar ${POCAO} à sacola` })).toBeDisabled()

  // Os valores do checkout ficam congelados.
  await loja.conferirValores({ subtotal: subtotalFinal, desconto: descontoFinal, total: subtotalFinal - descontoFinal })

  // --- Nova compra ------------------------------------------------------------------------------
  await loja.sacola.getByRole('button', { name: 'Nova compra' }).click()
  await expect(loja.sacola).toContainText('Sua sacola está vazia')
  await expect(loja.mercadoria(POCAO).getByRole('button', { name: `Adicionar ${POCAO} à sacola` })).toBeEnabled()
})
