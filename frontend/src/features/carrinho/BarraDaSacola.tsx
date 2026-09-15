import { useEffect, useState } from 'react'
import { Botao } from '../../shared/Botao'
import { pluralizar } from '../../shared/formatacao'
import { Moeda } from '../../shared/Moeda'
import styles from './BarraDaSacola.module.css'
import { ID_SACOLA, ID_TITULO_SACOLA } from './LivroCaixa'
import { useCarrinho } from './useCarrinho'

export function BarraDaSacola() {
  const { carrinho } = useCarrinho()
  const [sacolaNaTela, setSacolaNaTela] = useState(false)

  // Some quando o próprio livro-caixa já está visível.
  useEffect(() => {
    const sacola = document.getElementById(ID_SACOLA)
    if (!sacola || !('IntersectionObserver' in window)) {
      return
    }

    const observador = new IntersectionObserver(([entrada]) => setSacolaNaTela(entrada?.isIntersecting ?? false), {
      threshold: 0.1,
    })
    observador.observe(sacola)
    return () => observador.disconnect()
  }, [])

  const unidades = carrinho?.itens.reduce((soma, item) => soma + item.quantidade, 0) ?? 0

  const irParaSacola = () => {
    const reduzirMovimento = window.matchMedia('(prefers-reduced-motion: reduce)').matches
    document.getElementById(ID_SACOLA)?.scrollIntoView({ behavior: reduzirMovimento ? 'auto' : 'smooth' })
    document.getElementById(ID_TITULO_SACOLA)?.focus({ preventScroll: true })
  }

  return (
    <div className={`${styles.barra} ${sacolaNaTela ? styles.oculta : ''}`} aria-hidden={sacolaNaTela}>
      <p className={styles.resumo}>
        <span className={styles.unidades}>{pluralizar(unidades, 'item', 'itens')} na sacola</span>
        <span className={styles.total}>
          Total <Moeda valor={carrinho?.total ?? 0} />
        </span>
      </p>
      <Botao variante="latao" onClick={irParaSacola} tabIndex={sacolaNaTela ? -1 : 0}>
        Ver sacola
      </Botao>
    </div>
  )
}
