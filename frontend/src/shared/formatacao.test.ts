import { formatarDataHora, formatarMoeda, formatarPercentual, pluralizar } from './formatacao'

describe('formatação', () => {
  it.each([
    [0, '0,00'],
    [7.45, '7,45'],
    [1234.5, '1.234,50'],
    [181.89, '181,89'],
  ])('formata %d moedas como %s', (valor, esperado) => {
    expect(formatarMoeda(valor)).toBe(esperado)
  })

  it('formata percentuais sem casas desnecessárias', () => {
    expect(formatarPercentual(10)).toBe('10%')
    expect(formatarPercentual(12.5)).toBe('12,5%')
  })

  it('formata data e hora no padrão brasileiro', () => {
    expect(formatarDataHora('2026-09-15T17:32:00')).toBe('15/09/2026 às 17:32')
  })

  it('pluraliza pela quantidade', () => {
    expect(pluralizar(1, 'item', 'itens')).toBe('1 item')
    expect(pluralizar(3, 'item', 'itens')).toBe('3 itens')
  })
})
