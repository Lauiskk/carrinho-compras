import type { ItemCarrinho } from '../../api/tipos'
import { Botao } from '../../shared/Botao'
import { MensagemErro } from '../../shared/MensagemErro'
import { Moeda } from '../../shared/Moeda'
import styles from './LinhaDoItem.module.css'
import { SeletorQuantidade } from './SeletorQuantidade'
import { useAlterarQuantidade, useRemoverItem } from './useCarrinho'

type Props = {
  item: ItemCarrinho
  somenteLeitura: boolean
}

export function LinhaDoItem({ item, somenteLeitura }: Props) {
  const alterar = useAlterarQuantidade()
  const remover = useRemoverItem()
  const ocupado = alterar.isPending || remover.isPending

  return (
    <li className={styles.linha}>
      <div className={styles.principal}>
        <span className={styles.nome}>{item.descricaoProduto}</span>
        <span className={styles.pontilhado} aria-hidden="true" />
        <Moeda valor={item.precoItem} className={styles.valor} />
      </div>

      <div className={styles.detalhe}>
        <span className={styles.unitario}>
          {item.quantidade} × <Moeda valor={item.precoLiquidoUnitario} />
        </span>

        {!somenteLeitura && (
          <div className={styles.controles}>
            <SeletorQuantidade
              superficie="papel"
              compacto
              valor={item.quantidade}
              maximo={item.quantidadeEstoque}
              onAlterar={(quantidade) => alterar.mutate({ produtoId: item.produtoId, quantidade })}
              rotulo={item.descricaoProduto}
              desabilitado={ocupado}
            />
            <Botao
              variante="link"
              onClick={() => remover.mutate(item.produtoId)}
              disabled={ocupado}
              aria-label={`Remover ${item.descricaoProduto} da sacola`}
            >
              Remover
            </Botao>
          </div>
        )}
      </div>

      {!somenteLeitura && item.quantidade >= item.quantidadeEstoque && (
        <p className={styles.limite}>Todo o estoque disponível já está na sacola.</p>
      )}
      <MensagemErro erro={alterar.error ?? remover.error} className={styles.erro} />
    </li>
  )
}
