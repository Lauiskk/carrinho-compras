import { BarraDaSacola } from '../features/carrinho/BarraDaSacola'
import { LivroCaixa } from '../features/carrinho/LivroCaixa'
import { Catalogo } from '../features/catalogo/Catalogo'
import { IconeBalanca } from '../shared/icones'
import styles from './App.module.css'

export function App() {
  return (
    <div className={styles.loja}>
      <header className={styles.letreiro}>
        <IconeBalanca className={styles.brasao} />
        <div>
          <h1 className={styles.nome}>O Corvo e a Balança</h1>
          <p className={styles.lema}>Suprimentos para quem desce às masmorras</p>
        </div>
      </header>

      <main className={styles.balcao}>
        <Catalogo />
        <LivroCaixa />
      </main>

      <BarraDaSacola />
    </div>
  )
}
