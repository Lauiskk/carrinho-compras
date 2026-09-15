import { http as rota, HttpResponse } from 'msw'
import { problema } from '../test/dados'
import { servidor } from '../test/servidor'
import { ApiError, CODIGO_SEM_CONEXAO, http, MENSAGEM_SEM_CONEXAO } from './http'

describe('http', () => {
  it('devolve o JSON da resposta de sucesso', async () => {
    servidor.use(rota.get('/api/produtos', () => HttpResponse.json([{ id: 1 }])))

    await expect(http('GET', '/produtos')).resolves.toEqual([{ id: 1 }])
  })

  it('envia o corpo como JSON', async () => {
    let recebido: unknown
    servidor.use(
      rota.post('/api/carrinhos/c1/itens', async ({ request }) => {
        recebido = await request.json()
        return HttpResponse.json({})
      }),
    )

    await http('POST', '/carrinhos/c1/itens', { produtoId: 1, quantidade: 2 })

    expect(recebido).toEqual({ produtoId: 1, quantidade: 2 })
  })

  it('converte ProblemDetails em ApiError com status, código, detalhe e traceId', async () => {
    servidor.use(
      rota.put('/api/carrinhos/c1/cupom', () =>
        HttpResponse.json(problema(422, 'cupom.invalido', "O cupom 'XYZ' é inválido ou não existe."), {
          status: 422,
          headers: { 'Content-Type': 'application/problem+json' },
        }),
      ),
    )

    const erro = await http('PUT', '/carrinhos/c1/cupom', { codigoCupom: 'XYZ' }).catch((e: unknown) => e)

    expect(erro).toBeInstanceOf(ApiError)
    expect(erro).toMatchObject({ status: 422, code: 'cupom.invalido', traceId: '00-teste-00' })
    expect((erro as ApiError).mensagem).toBe("O cupom 'XYZ' é inválido ou não existe.")
  })

  it('usa a mensagem do primeiro campo inválido em erros de validação', async () => {
    servidor.use(
      rota.post('/api/carrinhos/c1/itens', () =>
        HttpResponse.json(
          problema(400, 'requisicao.invalida', 'Um ou mais campos da requisição são inválidos.', {
            quantidade: ['A quantidade deve ser maior que zero.'],
          }),
          { status: 400, headers: { 'Content-Type': 'application/problem+json' } },
        ),
      ),
    )

    const erro = (await http('POST', '/carrinhos/c1/itens', { produtoId: 1, quantidade: 0 }).catch((e: unknown) => e)) as ApiError

    expect(erro.mensagem).toBe('A quantidade deve ser maior que zero.')
  })

  it('trata falha de rede como API indisponível', async () => {
    servidor.use(rota.get('/api/produtos', () => HttpResponse.error()))

    const erro = (await http('GET', '/produtos').catch((e: unknown) => e)) as ApiError

    expect(erro.code).toBe(CODIGO_SEM_CONEXAO)
    expect(erro.mensagem).toBe(MENSAGEM_SEM_CONEXAO)
  })

  it('trata resposta de erro que não é ProblemDetails (ex.: proxy sem back-end) como API indisponível', async () => {
    servidor.use(rota.get('/api/produtos', () => new HttpResponse('Bad Gateway', { status: 502 })))

    const erro = (await http('GET', '/produtos').catch((e: unknown) => e)) as ApiError

    expect(erro.status).toBe(502)
    expect(erro.code).toBe(CODIGO_SEM_CONEXAO)
  })
})
