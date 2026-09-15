import { useState } from 'react'
import type { Produto } from '../../api/tipos'
import { Botao } from '../../shared/Botao'
import { MensagemErro } from '../../shared/MensagemErro'
import { Moeda } from '../../shared/Moeda'
import { SeletorQuantidade } from '../carrinho/SeletorQuantidade'
import { useAdicionarItem } from '../carrinho/useCarrinho'
import { IconeMercadoria } from './IconeMercadoria'
import styles from './Mercadoria.module.css'

type Props = {
  produto: Produto
  quantidadeNaSacola: number
  compraFinalizada: boolean
}

export function Mercadoria({ produto, quantidadeNaSacola, compraFinalizada }: Props) {
  const [quantidade, setQuantidade] = useState(1)
  const adicionar = useAdicionarItem()

  // Limite de interface para evitar um clique que já sabemos que falharia; a API continua validando o estoque.
  const disponivel = Math.max(produto.quantidadeEstoque - quantidadeNaSacola, 0)
  const quantidadeEscolhida = Math.min(quantidade, Math.max(disponivel, 1))
  const podeAdicionar = !compraFinalizada && disponivel > 0 && !adicionar.isPending

  const adicionarNaSacola = () =>
    adicionar.mutate(
      { produtoId: produto.id, quantidade: quantidadeEscolhida },
      { onSuccess: () => setQuantidade(1) },
    )

  return (
    <li className={styles.mercadoria}>
      <div className={styles.vitrine}>
        <IconeMercadoria produtoId={produto.id} descricao={produto.descricaoProduto} className={styles.icone} />
      </div>
      <div className={styles.tabua} aria-hidden="true" />

      <div className={styles.conteudo}>
        <p className={styles.etiqueta}>
          <Moeda valor={produto.precoLiquido} />
        </p>
        <h3 className={styles.nome}>{produto.descricaoProduto}</h3>
        <p className={`${styles.estoque} ${produto.quantidadeEstoque === 0 ? styles.esgotado : ''}`}>
          <TextoEstoque estoque={produto.quantidadeEstoque} />
          {quantidadeNaSacola > 0 && <span className={styles.naSacola}> ({quantidadeNaSacola} na sacola)</span>}
        </p>

        <div className={styles.acoes}>
          <SeletorQuantidade
            valor={quantidadeEscolhida}
            maximo={Math.max(disponivel, 1)}
            onAlterar={setQuantidade}
            rotulo={produto.descricaoProduto}
            desabilitado={!podeAdicionar}
          />
          <Botao
            variante="latao"
            className={styles.adicionar}
            onClick={adicionarNaSacola}
            disabled={!podeAdicionar}
            aria-label={`Adicionar ${produto.descricaoProduto} à sacola`}
          >
            {adicionar.isPending ? 'Adicionando…' : 'Adicionar'}
          </Botao>
        </div>

        <MensagemErro erro={adicionar.error} className={styles.erro} />
      </div>
    </li>
  )
}

function TextoEstoque({ estoque }: { estoque: number }) {
  if (estoque === 0) {
    return <>Esgotado</>
  }

  return <>{estoque === 1 ? 'Última unidade em estoque' : `${estoque} em estoque`}</>
}
