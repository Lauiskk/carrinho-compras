import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { SeletorQuantidade } from './SeletorQuantidade'

describe('SeletorQuantidade', () => {
  it('aumenta e diminui dentro dos limites', async () => {
    const onAlterar = vi.fn<(valor: number) => void>()
    render(<SeletorQuantidade valor={2} maximo={5} onAlterar={onAlterar} rotulo="Poção" />)

    await userEvent.click(screen.getByRole('button', { name: 'Aumentar quantidade de Poção' }))
    await userEvent.click(screen.getByRole('button', { name: 'Diminuir quantidade de Poção' }))

    expect(onAlterar).toHaveBeenNthCalledWith(1, 3)
    expect(onAlterar).toHaveBeenNthCalledWith(2, 1)
  })

  it('não deixa passar do mínimo (1) nem do estoque', () => {
    const { rerender } = render(<SeletorQuantidade valor={1} maximo={3} onAlterar={vi.fn<(valor: number) => void>()} rotulo="Poção" />)
    expect(screen.getByRole('button', { name: 'Diminuir quantidade de Poção' })).toBeDisabled()

    rerender(<SeletorQuantidade valor={3} maximo={3} onAlterar={vi.fn<(valor: number) => void>()} rotulo="Poção" />)
    expect(screen.getByRole('button', { name: 'Aumentar quantidade de Poção' })).toBeDisabled()
  })

  it('fica todo desabilitado quando pedido', () => {
    render(<SeletorQuantidade valor={2} maximo={5} onAlterar={vi.fn<(valor: number) => void>()} rotulo="Poção" desabilitado />)

    for (const botao of screen.getAllByRole('button')) {
      expect(botao).toBeDisabled()
    }
  })
})
