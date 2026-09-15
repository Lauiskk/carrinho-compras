import styles from './SeletorQuantidade.module.css'

type Props = {
  valor: number
  minimo?: number
  maximo: number
  onAlterar: (valor: number) => void
  /** Nome do produto, para os rótulos acessíveis ("Diminuir quantidade de ..."). */
  rotulo: string
  desabilitado?: boolean
  superficie?: 'balcao' | 'papel'
  compacto?: boolean
}

export function SeletorQuantidade({
  valor,
  minimo = 1,
  maximo,
  onAlterar,
  rotulo,
  desabilitado = false,
  superficie = 'balcao',
  compacto = false,
}: Props) {
  const classes = [styles.seletor, superficie === 'papel' ? styles.papel : '', compacto ? styles.compacto : '']
    .filter(Boolean)
    .join(' ')

  return (
    <fieldset className={classes}>
      <legend className="sr-only">Quantidade de {rotulo}</legend>
      <button
        type="button"
        className={styles.passo}
        onClick={() => onAlterar(valor - 1)}
        disabled={desabilitado || valor <= minimo}
        aria-label={`Diminuir quantidade de ${rotulo}`}
      >
        −
      </button>
      <output className={styles.valor}>{valor}</output>
      <button
        type="button"
        className={styles.passo}
        onClick={() => onAlterar(valor + 1)}
        disabled={desabilitado || valor >= maximo}
        aria-label={`Aumentar quantidade de ${rotulo}`}
      >
        +
      </button>
    </fieldset>
  )
}
