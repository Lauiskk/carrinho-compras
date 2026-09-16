import { expect, test } from '@playwright/test'
import { Loja } from '../suporte/loja.js'

test('um cupom inexistente é recusado sem derrubar o cupom que já estava aplicado', async ({ page }) => {
  const loja = new Loja(page)
  await loja.abrir()
  const mercadoria = await loja.escolherMercadoria(1)
  await loja.adicionar(mercadoria.descricaoProduto)
  await expect(loja.linhaDaSacola(mercadoria.descricaoProduto)).toBeVisible()

  await loja.aplicarCupom('10OFF')
  await expect(loja.sacola.getByText(/Cupom 10OFF aplicado/)).toBeVisible()

  await loja.aplicarCupom('NAOEXISTE')

  await expect(loja.sacola.getByRole('alert')).toHaveText("O cupom 'NAOEXISTE' é inválido ou não existe.")
  await expect(loja.sacola.getByText(/Cupom 10OFF aplicado/)).toBeVisible()
})

test('a interface não deixa passar do estoque disponível', async ({ page }) => {
  const loja = new Loja(page)
  await loja.abrir()

  // O teste cria a própria escassez: leva tudo o que está disponível de uma mercadoria.
  const mercadoria = await loja.escolherMercadoriaEscassa(2)
  const nome = mercadoria.descricaoProduto
  await loja.adicionar(nome, mercadoria.quantidadeDisponivel)

  await expect(loja.linhaDaSacola(nome)).toContainText('Todo o estoque disponível já está na sacola')
  await expect(loja.mercadoria(nome).getByRole('button', { name: `Adicionar ${nome} à sacola` })).toBeDisabled()
  await expect(loja.mercadoria(nome)).toContainText(`(${mercadoria.quantidadeDisponivel} na sacola)`)
  await expect(loja.mercadoria(nome)).toContainText('Esgotado')

  // Devolve o que o cenário segurou: a reserva expiraria sozinha, mas uma rodada seguida não precisa esperar.
  await loja.removerDaSacola(nome)
  await expect(loja.mercadoria(nome)).toContainText(`${mercadoria.quantidadeDisponivel} em estoque`)
})

test('não dá para finalizar uma sacola vazia', async ({ page }) => {
  const loja = new Loja(page)
  await loja.abrir()

  await expect(loja.sacola.getByRole('button', { name: 'Finalizar compra' })).toBeDisabled()
})
