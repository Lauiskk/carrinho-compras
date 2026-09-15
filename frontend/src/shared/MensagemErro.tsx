import { ApiError } from '../api/http'

type Props = {
  erro: unknown
  id?: string
  className?: string
}

/** Mensagem de falha junto da ação que falhou, com o texto vindo da API. */
export function MensagemErro({ erro, id, className }: Props) {
  if (!erro) {
    return null
  }

  const mensagem = erro instanceof ApiError ? erro.mensagem : 'Não foi possível concluir a operação. Tente de novo.'
  return (
    <p role="alert" id={id} className={className}>
      {mensagem}
    </p>
  )
}
