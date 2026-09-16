import { ApiError } from '../api/http'

type Props = {
  erro: unknown
  id?: string
  className?: string
}

/**
 * A mensagem da API é escrita para quem usa a loja e vai direto para a tela. A exceção é o carrinho que
 * não existe mais: "Carrinho '01a0...' não encontrado" diz respeito ao id, não à pessoa — aqui ela vira
 * um texto que explica o que aconteceu e o que fazer.
 */
const AMIGAVEIS: Record<string, string> = {
  'carrinho.nao_encontrado': 'Sua sacola anterior não existe mais. Escolha a mercadoria de novo para começar outra.',
}

/** Mensagem de falha junto da ação que falhou, com o texto vindo da API. */
export function MensagemErro({ erro, id, className }: Props) {
  if (!erro) {
    return null
  }

  const mensagem = erro instanceof ApiError
    ? (AMIGAVEIS[erro.code ?? ''] ?? erro.mensagem)
    : 'Não foi possível concluir a operação. Tente de novo.'

  return (
    <p role="alert" id={id} className={className}>
      {mensagem}
    </p>
  )
}
