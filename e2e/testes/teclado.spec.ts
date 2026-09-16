import { expect, test, type Locator, type Page } from '@playwright/test'
import { Loja } from '../suporte/loja.js'

/** Vai apertando Tab até o elemento receber o foco. Falha se ele não for alcançável pelo teclado. */
async function tabAte(page: Page, destino: Locator, limite = 40): Promise<void> {
  for (let passo = 0; passo < limite; passo += 1) {
    if (await destino.evaluate((elemento) => elemento === document.activeElement)) {
      return
    }
    await page.keyboard.press('Tab')
  }
  throw new Error(`O elemento não foi alcançado em ${limite} tabulações.`)
}

test('dá para comprar usando só o teclado', async ({ page }) => {
  const loja = new Loja(page)
  await loja.abrir()
  const POCAO = (await loja.escolherMercadoria(1)).descricaoProduto

  // Adicionar a mercadoria.
  const adicionar = loja.mercadoria(POCAO).getByRole('button', { name: `Adicionar ${POCAO} à sacola` })
  await tabAte(page, adicionar)
  await expect(adicionar).toBeFocused()
  await page.keyboard.press('Enter')
  await expect(loja.linhaDaSacola(POCAO)).toBeVisible()

  // Aplicar o cupom: digitar e enviar com Enter (o formulário responde ao Enter).
  const campoCupom = loja.sacola.getByRole('textbox')
  await tabAte(page, campoCupom)
  await page.keyboard.type('10OFF')
  await page.keyboard.press('Enter')
  await expect(loja.sacola.getByText(/Cupom 10OFF aplicado/)).toBeVisible()

  // Finalizar.
  const finalizar = loja.sacola.getByRole('button', { name: 'Finalizar compra' })
  await tabAte(page, finalizar)
  await page.keyboard.press('Enter')
  await expect(loja.sacola.getByText(/Compra finalizada em/)).toBeVisible()
})

test('o foco do teclado é visível', async ({ page }) => {
  const loja = new Loja(page)
  await loja.abrir()
  const POCAO = (await loja.escolherMercadoria(1)).descricaoProduto

  const adicionar = loja.mercadoria(POCAO).getByRole('button', { name: `Adicionar ${POCAO} à sacola` })
  await tabAte(page, adicionar)

  const contorno = await adicionar.evaluate((elemento) => {
    const estilo = getComputedStyle(elemento)
    return { largura: estilo.outlineWidth, estiloDaLinha: estilo.outlineStyle }
  })
  expect(contorno.estiloDaLinha).not.toBe('none')
  expect(Number.parseFloat(contorno.largura)).toBeGreaterThan(0)
})
