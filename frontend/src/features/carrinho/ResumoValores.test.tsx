import { render, screen } from '@testing-library/react'
import { ResumoValores } from './ResumoValores'

describe('ResumoValores', () => {
  it('exibe exatamente os valores calculados pela API', () => {
    // Valores propositalmente "inconsistentes": o front não recalcula nada, só mostra o que recebeu.
    render(<ResumoValores subtotal={213.99} desconto={32.1} total={181.89} cupom={{ codigoCupom: '15OFF', percentualDesconto: 15 }} />)

    expect(screen.getByText('Subtotal').nextElementSibling).toHaveTextContent('213,99 moedas de ouro')
    expect(screen.getByText('Desconto (15OFF, 15%)').nextElementSibling).toHaveTextContent('menos 32,10 moedas de ouro')
    expect(screen.getByText('Total').nextElementSibling).toHaveTextContent('181,89 moedas de ouro')
  })

  it('mostra desconto zerado quando não há cupom', () => {
    render(<ResumoValores subtotal={50} desconto={0} total={50} cupom={null} />)

    expect(screen.getByText('Desconto').nextElementSibling).toHaveTextContent('0,00 moedas de ouro')
  })
})
