import { expect, test } from '@playwright/test'
import { Loja, moeda } from '../suporte/loja.js'

const POCAO = 'Poção de Cura Menor'

test('no celular a loja cabe na tela e a barra leva até a sacola', async ({ page }) => {
  const loja = new Loja(page)
  await loja.abrir()

  // Nada de rolagem horizontal.
  const { largura, visivel } = await page.evaluate(() => ({
    largura: document.documentElement.scrollWidth,
    visivel: document.documentElement.clientWidth,
  }))
  expect(largura).toBeLessThanOrEqual(visivel)

  // Duas colunas de mercadorias.
  const colunas = await loja.prateleiras
    .getByRole('list')
    .evaluate((lista) => getComputedStyle(lista).gridTemplateColumns.split(' ').length)
  expect(colunas).toBe(2)

  // A sacola fica abaixo do catálogo, então há uma barra fixa com o resumo.
  // (O resumo é lido na própria barra: o mesmo texto existe no anúncio para leitor de tela.)
  const verSacola = page.getByRole('button', { name: 'Ver sacola' })
  const barra = verSacola.locator('..')
  await expect(verSacola).toBeVisible()

  const preco = await loja.precoDeCatalogo(POCAO)
  await loja.adicionar(POCAO)
  await expect(barra).toContainText('1 item na sacola')
  await expect(barra).toContainText(`Total ${moeda(preco)}`)

  // "Ver sacola" rola até a sacola e leva o foco para o título dela.
  await verSacola.click()
  await expect(loja.sacola.getByRole('heading', { name: 'Sua sacola' })).toBeFocused()
  await expect(loja.linhaDaSacola(POCAO)).toBeInViewport()
})
