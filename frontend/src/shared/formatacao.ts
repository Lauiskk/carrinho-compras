const moeda = new Intl.NumberFormat('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
const percentual = new Intl.NumberFormat('pt-BR', { maximumFractionDigits: 2 })
const data = new Intl.DateTimeFormat('pt-BR', { day: '2-digit', month: '2-digit', year: 'numeric' })
const hora = new Intl.DateTimeFormat('pt-BR', { hour: '2-digit', minute: '2-digit' })

/** 1234.5 → "1.234,50" (os valores são em moedas de ouro; o símbolo é desenhado à parte). */
export function formatarMoeda(valor: number): string {
  return moeda.format(valor)
}

/** 10 → "10%"; 12.5 → "12,5%". */
export function formatarPercentual(valor: number): string {
  return `${percentual.format(valor)}%`
}

/** "2026-09-15T17:32:00Z" → "15/09/2026 às 14:32" (no fuso do navegador). */
export function formatarDataHora(iso: string): string {
  const instante = new Date(iso)
  return `${data.format(instante)} às ${hora.format(instante)}`
}

export function pluralizar(quantidade: number, singular: string, plural: string): string {
  return `${quantidade} ${quantidade === 1 ? singular : plural}`
}
