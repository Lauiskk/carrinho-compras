import type { CupomAplicado } from '../../api/tipos'
import { formatarPercentual } from '../../shared/formatacao'
import { Moeda } from '../../shared/Moeda'
import styles from './ResumoValores.module.css'

type Props = {
  subtotal: number
  desconto: number
  total: number
  cupom: CupomAplicado | null
}

/** Exibe os valores exatamente como a API calculou. */
export function ResumoValores({ subtotal, desconto, total, cupom }: Props) {
  return (
    <dl className={styles.resumo}>
      <div className={styles.linha}>
        <dt>Subtotal</dt>
        <dd>
          <Moeda valor={subtotal} />
        </dd>
      </div>
      <div className={styles.linha}>
        <dt>Desconto{cupom ? ` (${cupom.codigoCupom}, ${formatarPercentual(cupom.percentualDesconto)})` : ''}</dt>
        <dd>
          <Moeda valor={desconto} deducao={desconto > 0} />
        </dd>
      </div>
      <div className={`${styles.linha} ${styles.total}`}>
        <dt>Total</dt>
        <dd>
          <Moeda valor={total} />
        </dd>
      </div>
    </dl>
  )
}
