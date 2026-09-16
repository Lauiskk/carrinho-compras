import { useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'
import styles from './PrazoDaReserva.module.css'

type Props = {
  /** Instante (ISO) em que o mercador devolve as mercadorias à loja. */
  expiraEm: string
}

const MINUTO = 60_000
const INTERVALO = 15_000

/**
 * Enquanto as mercadorias estão na sacola, elas saem da vitrine para todo mundo. Mostrar por quanto tempo
 * isso vale é o que torna a reserva visível: sem esta linha, o número do estoque mudaria sozinho e ninguém
 * entenderia por quê.
 *
 * O componente é montado com `key={expiraEm}`: a cada alteração da sacola o prazo é outro e a contagem
 * recomeça do zero, sem precisar sincronizar estado dentro de efeito.
 */
export function PrazoDaReserva({ expiraEm }: Props) {
  const queryClient = useQueryClient()
  const [restante, setRestante] = useState(() => faltam(expiraEm))

  useEffect(() => {
    const relogio = setInterval(() => setRestante(faltam(expiraEm)), INTERVALO)
    return () => clearInterval(relogio)
  }, [expiraEm])

  useEffect(() => {
    if (restante > 0) {
      return
    }

    // Venceu. Quem encerra a sacola é o servidor (a varredura em segundo plano, ou a próxima alteração),
    // então a tela pergunta de novo a cada passagem até a resposta mudar — parar na primeira tentativa
    // deixaria a pessoa presa neste aviso, sem saída, até recarregar a página.
    void queryClient.invalidateQueries({ queryKey: ['carrinho'] })
    void queryClient.invalidateQueries({ queryKey: ['produtos'] })
  }, [restante, queryClient])

  if (restante <= 0) {
    return <p className={`${styles.prazo} ${styles.vencendo}`}>O mercador está recolhendo suas mercadorias…</p>
  }

  const minutos = Math.ceil(restante / MINUTO)
  return (
    <p className={styles.prazo}>
      <span className={styles.ampulheta} aria-hidden="true" />O mercador guarda estas peças por{' '}
      <strong>{minutos === 1 ? 'mais 1 minuto' : `mais ${minutos} minutos`}</strong>.
    </p>
  )
}

function faltam(expiraEm: string): number {
  return new Date(expiraEm).getTime() - Date.now()
}
