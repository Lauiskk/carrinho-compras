import { useId } from 'react'
import styles from './SeloDeCera.module.css'

type Ponto = { x: number; y: number }

const formatar = (valor: number) => valor.toFixed(1)
const meio = (a: Ponto, b: Ponto): Ponto => ({ x: (a.x + b.x) / 2, y: (a.y + b.y) / 2 })

// Contorno irregular de cera derretida: raio variando em torno do círculo, suavizado com curvas quadráticas.
const CONTORNO = (() => {
  const total = 30
  const pontos = Array.from({ length: total }, (_, indice): Ponto => {
    const angulo = (indice / total) * Math.PI * 2
    const raio = 71 + Math.sin(indice * 2.3) * 3.2 + (indice % 3 === 0 ? 2.6 : 0)
    return { x: 80 + Math.cos(angulo) * raio, y: 80 + Math.sin(angulo) * raio }
  })

  const primeiro = pontos[0] as Ponto
  const ultimo = pontos[total - 1] as Ponto
  const inicio = meio(ultimo, primeiro)
  const curvas = pontos.map((ponto, indice) => {
    const destino = meio(ponto, pontos[(indice + 1) % total] as Ponto)
    return `Q${formatar(ponto.x)} ${formatar(ponto.y)} ${formatar(destino.x)} ${formatar(destino.y)}`
  })

  return `M${formatar(inicio.x)} ${formatar(inicio.y)}${curvas.join('')}Z`
})()

/** Selo de cera carimbado sobre o livro-caixa quando a compra é finalizada. */
export function SeloDeCera() {
  const id = useId()
  const gradiente = `cera-${id}`
  const arco = `arco-${id}`

  // Decorativo: o texto "Compra finalizada em ..." ao lado já comunica o estado.
  return (
    <div className={styles.selo}>
      <svg viewBox="0 0 160 160" aria-hidden="true" focusable="false">
        <defs>
          <radialGradient id={gradiente} cx="38%" cy="32%" r="75%">
            <stop offset="0" stopColor="#cf5a60" />
            <stop offset="0.5" stopColor="#962a36" />
            <stop offset="1" stopColor="#4f1019" />
          </radialGradient>
          <path id={arco} d="M34 84a46 46 0 0 1 92 0" />
        </defs>

        <path d={CONTORNO} fill={`url(#${gradiente})`} />
        <circle cx="80" cy="80" r="53" fill="none" stroke="#4f1019" strokeWidth="3.5" opacity="0.55" />
        <circle cx="80" cy="81.5" r="48" fill="none" stroke="#f2b8b0" strokeWidth="1.2" opacity="0.35" />

        {/* Relevo: a mesma forma em sombra (abaixo) e em luz (acima) */}
        <g fill="none" strokeLinecap="round" strokeLinejoin="round" strokeWidth="3.2">
          <g stroke="#3d0b12" opacity="0.55" transform="translate(0 1.4)">
            <path d="M80 72v34M71 106h18M62 80h36" />
            <path d="M62 80l-7 13a7 7 0 0 0 14 0zM98 80l-7 13a7 7 0 0 0 14 0z" />
          </g>
          <g stroke="#f4cdc4" opacity="0.82">
            <path d="M80 72v34M71 106h18M62 80h36" />
            <path d="M62 80l-7 13a7 7 0 0 0 14 0zM98 80l-7 13a7 7 0 0 0 14 0z" />
          </g>
        </g>

        <text className={styles.texto} fill="#f4cdc4" opacity="0.9">
          <textPath href={`#${arco}`} startOffset="50%" textAnchor="middle">
            Finalizado
          </textPath>
        </text>
      </svg>
    </div>
  )
}
