import { expect, type Locator, type Page } from '@playwright/test'

/** Produto como o catálogo da API devolve. */
export type Mercadoria = {
  id: number
  descricaoProduto: string
  precoLiquido: number
  quantidadeEstoque: number
  quantidadeReservada: number
  quantidadeDisponivel: number
}

/**
 * Atalhos para falar com a loja pelos mesmos nomes que uma pessoa usaria: "a prateleira da Poção",
 * "a linha da sacola", "o Total". Tudo por papel e rótulo acessível — se um controle perder o nome,
 * o teste quebra, então isto também funciona como checagem de acessibilidade.
 */
export class Loja {
  constructor(private readonly page: Page) {}

  get prateleiras(): Locator {
    return this.page.getByRole('region', { name: 'Mercadorias' })
  }

  get sacola(): Locator {
    return this.page.getByRole('complementary', { name: 'Sua sacola' })
  }

  async abrir(): Promise<void> {
    await this.page.goto('/')
    // A loja só está pronta quando o catálogo chegou da API.
    await expect(this.prateleiras.getByRole('listitem')).not.toHaveCount(0)
  }

  /** O card de uma mercadoria no catálogo. */
  mercadoria(nome: string): Locator {
    return this.prateleiras.getByRole('listitem').filter({ has: this.page.getByRole('heading', { name: nome }) })
  }

  /** A linha de um produto dentro da sacola. */
  linhaDaSacola(nome: string): Locator {
    return this.sacola.getByRole('listitem').filter({ hasText: nome })
  }

  /**
   * Escolhe uma mercadoria com folga suficiente <b>agora</b>, perguntando ao catálogo da API. Com reserva de
   * estoque os números são vivos (outra sacola pode estar segurando peças, e cada compra baixa o físico),
   * então fixar um produto no teste o tornaria frágil.
   */
  async escolherMercadoria(minimoDisponivel: number, excluir: string[] = []): Promise<Mercadoria> {
    const produtos = await this.catalogo()
    const candidatos = produtos
      .filter((produto) => produto.quantidadeDisponivel >= minimoDisponivel && !excluir.includes(produto.descricaoProduto))
      .sort((a, b) => b.quantidadeDisponivel - a.quantidadeDisponivel)

    const escolhida = candidatos[0]
    if (!escolhida) {
      throw new Error(`Nenhuma mercadoria com ${minimoDisponivel} unidade(s) disponível(is) no momento.`)
    }
    return escolhida
  }

  /**
   * A mercadoria mais escassa que ainda atende ao mínimo. Serve aos cenários que precisam esgotar o
   * disponível: esvaziar a prateleira de 2 unidades leva um clique; a de 50, quarenta e nove.
   */
  async escolherMercadoriaEscassa(minimoDisponivel: number): Promise<Mercadoria> {
    const candidatas = await this.catalogo()
    const escolhida = candidatas
      .filter((produto) => produto.quantidadeDisponivel >= minimoDisponivel)
      .sort((a, b) => a.quantidadeDisponivel - b.quantidadeDisponivel)[0]

    if (!escolhida) {
      throw new Error(`Nenhuma mercadoria com ${minimoDisponivel} unidade(s) disponível(is) no momento.`)
    }
    return escolhida
  }

  private async catalogo(): Promise<Mercadoria[]> {
    const resposta = await this.page.request.get('/api/produtos')
    expect(resposta.ok(), 'o catálogo precisa responder para o teste escolher a mercadoria').toBeTruthy()
    return (await resposta.json()) as Mercadoria[]
  }

  /** Preço de catálogo da mercadoria, em centavos, lido da própria etiqueta. */
  async precoDeCatalogo(nome: string): Promise<number> {
    const etiqueta = await leitura(this.mercadoria(nome)).first().textContent()
    return centavos((etiqueta ?? '').replace('moedas de ouro', '').trim())
  }

  async adicionar(nome: string, quantidade = 1): Promise<void> {
    const card = this.mercadoria(nome)
    for (let clique = 1; clique < quantidade; clique += 1) {
      await card.getByRole('button', { name: `Aumentar quantidade de ${nome}` }).click()
    }
    await card.getByRole('button', { name: `Adicionar ${nome} à sacola` }).click()
  }

  /** Ajusta a quantidade na sacola até o valor pedido, clicando no seletor da linha. */
  async definirQuantidadeNaSacola(nome: string, de: number, para: number): Promise<void> {
    const linha = this.linhaDaSacola(nome)
    const botao = de < para ? `Aumentar quantidade de ${nome}` : `Diminuir quantidade de ${nome}`
    for (let passo = 0; passo < Math.abs(para - de); passo += 1) {
      await linha.getByRole('button', { name: botao }).click()
      await expect(linha.getByRole('status')).toHaveText(String(de + (de < para ? passo + 1 : -passo - 1)))
    }
  }

  async removerDaSacola(nome: string): Promise<void> {
    await this.linhaDaSacola(nome).getByRole('button', { name: `Remover ${nome} da sacola` }).click()
  }

  async aplicarCupom(codigo: string): Promise<void> {
    await this.sacola.getByRole('textbox').fill(codigo)
    await this.sacola.getByRole('button', { name: 'Aplicar' }).click()
  }

  async removerCupom(): Promise<void> {
    await this.sacola.getByRole('button', { name: 'Remover cupom' }).click()
  }

  async finalizar(): Promise<void> {
    await this.sacola.getByRole('button', { name: 'Finalizar compra' }).click()
  }

  /**
   * O valor de uma linha do resumo. O `ResumoValores` sempre desenha as três linhas nesta ordem;
   * os testes conferem os rótulos antes de ler os valores, então a suposição não passa despercebida.
   */
  valor(qual: 'subtotal' | 'desconto' | 'total'): Locator {
    const posicao = { subtotal: 0, desconto: 1, total: 2 }[qual]
    return leitura(this.sacola.getByRole('definition').nth(posicao))
  }

  /** Confere que o resumo está na ordem esperada — guarda as posições usadas em `valor`. */
  async conferirRotulosDoResumo(): Promise<void> {
    await expect(this.sacola.getByRole('term').nth(0)).toHaveText('Subtotal')
    await expect(this.sacola.getByRole('term').nth(1)).toHaveText(/^Desconto/)
    await expect(this.sacola.getByRole('term').nth(2)).toHaveText('Total')
  }

  /** Confere subtotal, desconto e total de uma vez, em centavos. */
  async conferirValores({ subtotal, desconto, total }: { subtotal: number; desconto: number; total: number }): Promise<void> {
    await expect(this.valor('subtotal')).toHaveText(`${moeda(subtotal)} moedas de ouro`)
    await expect(this.valor('desconto')).toHaveText(`${desconto > 0 ? 'menos ' : ''}${moeda(desconto)} moedas de ouro`)
    await expect(this.valor('total')).toHaveText(`${moeda(total)} moedas de ouro`)
  }

  /** O preço do item (preço unitário × quantidade) é o primeiro valor da linha. */
  precoDoItem(nome: string): Locator {
    return leitura(this.linhaDaSacola(nome)).first()
  }
}

/** O texto que um leitor de tela ouve num valor em moedas (o número visível é aria-hidden). */
function leitura(dentro: Locator): Locator {
  return dentro.locator('span.sr-only')
}

/** "1.234,50" → 123450 centavos. */
export function centavos(texto: string): number {
  return Math.round(Number(texto.replace(/\./g, '').replace(',', '.')) * 100)
}

/** 123450 centavos → "1.234,50", como o front formata. */
export function moeda(centavos: number): string {
  return (centavos / 100).toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

/**
 * Desconto esperado, calculado aqui de forma independente da API: percentual sobre o subtotal,
 * arredondado para 2 casas com a regra comercial (meio para longe de zero). Em centavos inteiros,
 * para o teste não herdar a imprecisão de ponto flutuante do JavaScript.
 */
export function descontoEsperado(subtotalEmCentavos: number, percentual: number): number {
  return Math.round((subtotalEmCentavos * percentual) / 100)
}
