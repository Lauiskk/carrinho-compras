import { useId, useState, type FormEvent } from 'react'
import type { CupomAplicado } from '../../api/tipos'
import { Botao } from '../../shared/Botao'
import { formatarPercentual } from '../../shared/formatacao'
import { MensagemErro } from '../../shared/MensagemErro'
import styles from './FormularioCupom.module.css'
import { useAplicarCupom, useRemoverCupom } from './useCarrinho'

type Props = {
  cupom: CupomAplicado | null
}

export function FormularioCupom({ cupom }: Props) {
  const [codigo, setCodigo] = useState('')
  const aplicar = useAplicarCupom()
  const remover = useRemoverCupom()
  const idCampo = useId()
  const idErro = useId()
  const erro = aplicar.error ?? remover.error

  const aplicarCupom = (evento: FormEvent<HTMLFormElement>) => {
    evento.preventDefault()
    // A validação (código vazio, cupom inexistente) fica com a API: a mensagem dela é exibida abaixo.
    aplicar.mutate(codigo, { onSuccess: () => setCodigo('') })
  }

  return (
    <div className={styles.cupom}>
      {cupom && (
        <div className={styles.aplicado}>
          <span className={styles.lacre} aria-hidden="true" />
          <p>
            Cupom <strong>{cupom.codigoCupom}</strong> aplicado ({formatarPercentual(cupom.percentualDesconto)} de desconto)
          </p>
          <Botao variante="link" onClick={() => remover.mutate()} disabled={remover.isPending}>
            Remover cupom
          </Botao>
        </div>
      )}

      <form onSubmit={aplicarCupom} noValidate>
        <label htmlFor={idCampo} className={styles.rotulo}>
          {cupom ? 'Trocar cupom' : 'Cupom de desconto'}
        </label>
        <div className={styles.campoEBotao}>
          <input
            id={idCampo}
            className={styles.campo}
            value={codigo}
            onChange={(evento) => setCodigo(evento.target.value)}
            placeholder="Ex.: 10OFF"
            autoComplete="off"
            spellCheck={false}
            aria-invalid={aplicar.isError}
            aria-describedby={erro ? idErro : undefined}
          />
          <Botao type="submit" variante="tinta" disabled={aplicar.isPending}>
            {aplicar.isPending ? 'Aplicando…' : 'Aplicar'}
          </Botao>
        </div>
        <MensagemErro erro={erro} id={idErro} className={styles.erro} />
      </form>
    </div>
  )
}
