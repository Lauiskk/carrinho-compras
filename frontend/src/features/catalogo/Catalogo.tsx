import { Botao } from '../../shared/Botao'
import { MensagemErro } from '../../shared/MensagemErro'
import { useCarrinhoContexto } from '../carrinho/carrinhoContexto'
import { useCarrinho } from '../carrinho/useCarrinho'
import styles from './Catalogo.module.css'
import { Mercadoria } from './Mercadoria'
import { useProdutos } from './useProdutos'

export function Catalogo() {
  const produtos = useProdutos()
  const { carrinho } = useCarrinho()
  const { esquecerCarrinho } = useCarrinhoContexto()
  const compraFinalizada = carrinho?.status === 'Finalizado'

  const quantidadeNaSacola = (produtoId: number) =>
    carrinho?.itens.find((item) => item.produtoId === produtoId)?.quantidade ?? 0

  return (
    <section className={styles.catalogo} aria-labelledby="titulo-mercadorias">
      <h2 id="titulo-mercadorias" className={styles.titulo}>
        Mercadorias
      </h2>

      {compraFinalizada && (
        <div className={styles.finalizada}>
          <p>Esta compra foi finalizada e não aceita novas mercadorias.</p>
          <Botao variante="latao" onClick={esquecerCarrinho}>
            Nova compra
          </Botao>
        </div>
      )}

      {produtos.isPending && <p className={styles.aviso}>Carregando mercadorias…</p>}

      {produtos.isError && (
        <div className={styles.aviso}>
          <p>Não foi possível carregar as mercadorias.</p>
          <MensagemErro erro={produtos.error} />
          <Botao variante="latao" onClick={() => void produtos.refetch()}>
            Tentar de novo
          </Botao>
        </div>
      )}

      {produtos.isSuccess && (
        <ul className={styles.prateleiras}>
          {produtos.data.map((produto) => (
            <Mercadoria
              key={produto.id}
              produto={produto}
              quantidadeNaSacola={quantidadeNaSacola(produto.id)}
              compraFinalizada={compraFinalizada}
            />
          ))}
        </ul>
      )}
    </section>
  )
}
