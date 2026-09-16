import type { Carrinho } from '../../api/tipos'
import { Botao } from '../../shared/Botao'
import { formatarDataHora, formatarMoeda, pluralizar } from '../../shared/formatacao'
import { MensagemErro } from '../../shared/MensagemErro'
import { useCarrinhoContexto } from './carrinhoContexto'
import { FormularioCupom } from './FormularioCupom'
import { LinhaDoItem } from './LinhaDoItem'
import { PrazoDaReserva } from './PrazoDaReserva'
import styles from './LivroCaixa.module.css'
import { ResumoValores } from './ResumoValores'
import { SeloDeCera } from './SeloDeCera'
import { useCarrinho, useFinalizarCarrinho } from './useCarrinho'

export const ID_SACOLA = 'sacola'
export const ID_TITULO_SACOLA = 'titulo-sacola'

export function LivroCaixa() {
  const { carrinho, carregando, erro, recarregar } = useCarrinho()
  const { esquecerCarrinho } = useCarrinhoContexto()
  const finalizar = useFinalizarCarrinho()

  const itens = carrinho?.itens ?? []
  const finalizado = carrinho?.status === 'Finalizado'
  const expirado = carrinho?.status === 'Expirado'
  // Finalizada ou expirada, a sacola não aceita mais nenhuma alteração.
  const encerrada = finalizado || expirado

  return (
    <aside id={ID_SACOLA} className={styles.sacola} aria-labelledby={ID_TITULO_SACOLA}>
      <div className={`${styles.livro} ${finalizado ? styles.selado : ''}`}>
        <header className={styles.cabecalho}>
          <h2 id={ID_TITULO_SACOLA} className={styles.titulo} tabIndex={-1}>
            Sua sacola
          </h2>
          {carrinho && <p className={styles.numero}>Carrinho nº {carrinho.id.slice(0, 8)}</p>}
        </header>

        {carregando && <p className={styles.aviso}>Carregando sua sacola…</p>}

        {!carregando && erro && !carrinho && (
          <div className={styles.aviso}>
            <MensagemErro erro={erro} className={styles.erro} />
            <Botao variante="tinta" onClick={() => void recarregar()}>
              Tentar de novo
            </Botao>
          </div>
        )}

        {!carregando && !erro && itens.length === 0 && (
          <p className={styles.aviso}>Sua sacola está vazia. Escolha uma mercadoria para começar.</p>
        )}

        {itens.length > 0 && (
          <ul className={styles.itens}>
            {itens.map((item) => (
              <LinhaDoItem key={item.produtoId} item={item} somenteLeitura={encerrada} />
            ))}
          </ul>
        )}

        {!encerrada && <FormularioCupom cupom={carrinho?.cupom ?? null} />}

        <ResumoValores
          subtotal={carrinho?.subtotal ?? 0}
          desconto={carrinho?.desconto ?? 0}
          total={carrinho?.total ?? 0}
          cupom={carrinho?.cupom ?? null}
        />

        {finalizado && (
          <div className={styles.encerramento}>
            {carrinho.finalizadoEm && <p>Compra finalizada em {formatarDataHora(carrinho.finalizadoEm)}.</p>}
            <Botao variante="tinta" onClick={esquecerCarrinho}>
              Nova compra
            </Botao>
            <SeloDeCera />
          </div>
        )}

        {expirado && (
          <div className={styles.encerramento}>
            <p className={styles.expirada}>
              O mercador recolheu estas peças: a sacola ficou parada tempo demais e elas voltaram para a loja.
            </p>
            <Botao variante="tinta" onClick={esquecerCarrinho}>
              Começar de novo
            </Botao>
          </div>
        )}

        {!encerrada && (
          <div className={styles.finalizar}>
            {carrinho?.expiraEm && <PrazoDaReserva key={carrinho.expiraEm} expiraEm={carrinho.expiraEm} />}
            <Botao
              variante="cera"
              largo
              onClick={() => finalizar.mutate()}
              disabled={itens.length === 0 || finalizar.isPending}
            >
              {finalizar.isPending ? 'Finalizando…' : 'Finalizar compra'}
            </Botao>
            <MensagemErro erro={finalizar.error} className={styles.erro} />
          </div>
        )}

        <p className="sr-only" aria-live="polite">
          {carrinho ? anunciar(carrinho) : ''}
        </p>
      </div>
    </aside>
  )
}

function anunciar(carrinho: Carrinho): string {
  const unidades = carrinho.itens.reduce((soma, item) => soma + item.quantidade, 0)
  const total = `Total de ${formatarMoeda(carrinho.total)} moedas de ouro.`
  if (carrinho.status === 'Finalizado') {
    return `Compra finalizada. ${total}`
  }

  if (carrinho.status === 'Expirado') {
    return 'Sua sacola expirou e as mercadorias voltaram para a loja.'
  }

  return `${pluralizar(unidades, 'item', 'itens')} na sacola. ${total}`
}
