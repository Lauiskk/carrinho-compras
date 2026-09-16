import { expect, test } from '@playwright/test'
import { Loja } from '../suporte/loja.js'

test('o que entra na sacola sai da vitrine e volta ao sair', async ({ page }) => {
  const loja = new Loja(page)
  await loja.abrir()
  const mercadoria = await loja.escolherMercadoria(3)
  const nome = mercadoria.descricaoProduto
  const antes = mercadoria.quantidadeDisponivel

  await loja.adicionar(nome, 3)
  await expect(loja.linhaDaSacola(nome)).toBeVisible()
  await expect(loja.mercadoria(nome)).toContainText(textoDeEstoque(antes - 3))

  await loja.definirQuantidadeNaSacola(nome, 3, 1)
  await expect(loja.mercadoria(nome)).toContainText(textoDeEstoque(antes - 1))

  await loja.removerDaSacola(nome)
  await expect(loja.linhaDaSacola(nome)).toHaveCount(0)
  await expect(loja.mercadoria(nome)).toContainText(textoDeEstoque(antes))
})

test('a sacola diz por quanto tempo as mercadorias ficam guardadas', async ({ page }) => {
  const loja = new Loja(page)
  await loja.abrir()
  await expect(loja.sacola.getByText(/O mercador guarda estas peças/)).toHaveCount(0)

  const mercadoria = await loja.escolherMercadoria(1)
  await loja.adicionar(mercadoria.descricaoProduto)

  await expect(loja.sacola.getByText(/O mercador guarda estas peças por/)).toBeVisible()

  // Esvaziar solta as unidades e, com elas, o prazo.
  await loja.removerDaSacola(mercadoria.descricaoProduto)
  await expect(loja.sacola.getByText(/O mercador guarda estas peças/)).toHaveCount(0)
})

/**
 * O que a vitrine mostra: "Esgotado", "Última unidade em estoque" ou "N em estoque".
 * Expressão regular com borda à esquerda porque "3 em estoque" é substring de "13 em estoque".
 */
function textoDeEstoque(disponivel: number): RegExp {
  if (disponivel === 0) {
    return /Esgotado/
  }
  return disponivel === 1 ? /Última unidade em estoque/ : new RegExp(`(^|\\D)${disponivel} em estoque`)
}
