/** Erro devolvido pela API no formato ProblemDetails (RFC 9457), ou falha de rede. */
export class ApiError extends Error {
  readonly status: number
  readonly code: string
  readonly errors: Record<string, string[]>
  readonly traceId: string | undefined

  constructor(status: number, code: string, detail: string, errors: Record<string, string[]> = {}, traceId?: string) {
    super(detail)
    this.name = 'ApiError'
    this.status = status
    this.code = code
    this.errors = errors
    this.traceId = traceId
  }

  /** Mensagem para exibir: a do primeiro campo inválido, se houver, ou o detalhe do problema. */
  get mensagem(): string {
    const primeiroErroDeCampo = Object.values(this.errors).flat()[0]
    return primeiroErroDeCampo ?? this.message
  }
}

export const CODIGO_SEM_CONEXAO = 'rede.indisponivel'
export const MENSAGEM_SEM_CONEXAO = 'Sem conexão com a API. Verifique se o back-end está rodando e tente de novo.'

type ProblemDetails = {
  detail?: string
  code?: string
  errors?: Record<string, string[]>
  traceId?: string
}

type Metodo = 'GET' | 'POST' | 'PUT' | 'DELETE'

/** Única porta de saída para a API. Converte qualquer falha em {@link ApiError}. */
export async function http<T>(metodo: Metodo, caminho: string, corpo?: unknown, signal?: AbortSignal): Promise<T> {
  const url = new URL(`/api${caminho}`, window.location.href)
  const headers: Record<string, string> = { Accept: 'application/json' }
  if (corpo !== undefined) {
    headers['Content-Type'] = 'application/json'
  }

  let resposta: Response
  try {
    resposta = await fetch(url, {
      method: metodo,
      headers,
      body: corpo === undefined ? undefined : JSON.stringify(corpo),
      signal,
    })
  } catch (erro) {
    if (erro instanceof DOMException && erro.name === 'AbortError') {
      throw erro
    }
    throw new ApiError(0, CODIGO_SEM_CONEXAO, MENSAGEM_SEM_CONEXAO)
  }

  if (resposta.ok) {
    return (await resposta.json()) as T
  }

  const problema = await lerProblema(resposta)
  if (!problema) {
    // Sem ProblemDetails, a resposta não veio da API (ex.: proxy sem back-end disponível).
    throw new ApiError(resposta.status, CODIGO_SEM_CONEXAO, MENSAGEM_SEM_CONEXAO)
  }

  throw new ApiError(
    resposta.status,
    problema.code ?? 'erro.inesperado',
    problema.detail ?? 'Não foi possível concluir a operação.',
    problema.errors ?? {},
    problema.traceId,
  )
}

async function lerProblema(resposta: Response): Promise<ProblemDetails | null> {
  const tipo = resposta.headers.get('content-type') ?? ''
  if (!tipo.includes('json')) {
    return null
  }

  try {
    return (await resposta.json()) as ProblemDetails
  } catch {
    return null
  }
}
