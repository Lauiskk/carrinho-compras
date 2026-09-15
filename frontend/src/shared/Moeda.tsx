import { formatarMoeda } from './formatacao'
import { IconeMoeda } from './icones'
import styles from './Moeda.module.css'

type Props = {
  valor: number
  /** Exibe o valor como dedução (ex.: desconto). */
  deducao?: boolean
  className?: string
}

/** Valor em moedas de ouro: número formatado + moeda desenhada, com leitura acessível por extenso. */
export function Moeda({ valor, deducao = false, className }: Props) {
  const texto = formatarMoeda(valor)
  const classes = className ? `${styles.moeda} ${className}` : styles.moeda

  return (
    <span className={classes}>
      <span aria-hidden="true">
        {deducao ? '−' : ''}
        {texto}
      </span>
      <IconeMoeda className={styles.icone} />
      <span className="sr-only">
        {deducao ? 'menos ' : ''}
        {texto} moedas de ouro
      </span>
    </span>
  )
}
