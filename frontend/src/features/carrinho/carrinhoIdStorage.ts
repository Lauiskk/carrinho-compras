const CHAVE = 'corvo-e-balanca.carrinhoId'

/** Guarda o id do carrinho no navegador: a sacola sobrevive a recarregar a página. */
export const carrinhoIdStorage = {
  ler(): string | null {
    try {
      return window.localStorage.getItem(CHAVE)
    } catch {
      return null
    }
  },

  gravar(carrinhoId: string): void {
    try {
      window.localStorage.setItem(CHAVE, carrinhoId)
    } catch {
      // Armazenamento indisponível (ex.: modo privado): a sacola vale só para esta visita.
    }
  },

  limpar(): void {
    try {
      window.localStorage.removeItem(CHAVE)
    } catch {
      // Idem.
    }
  },
}
