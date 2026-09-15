import type { ReactNode } from 'react'

type Props = {
  produtoId: number
  descricao: string
  className?: string
}

type TipoDeMercadoria =
  | 'pocao'
  | 'lamina'
  | 'capa'
  | 'lanterna'
  | 'corda'
  | 'pergaminho'
  | 'escudo'
  | 'bota'
  | 'racao'
  | 'amuleto'

// A ilustração é escolhida pela descrição (não pelo id): se o catálogo for trocado, um produto
// desconhecido ganha um sigilo rúnico próprio em vez de um desenho errado.
const PALAVRAS: ReadonlyArray<[TipoDeMercadoria, RegExp]> = [
  ['pocao', /poc|elixir|frasco/],
  ['lamina', /adaga|espada|faca|lamina/],
  ['capa', /capa|manto/],
  ['lanterna', /lanterna|lampiao|tocha/],
  ['corda', /corda/],
  ['pergaminho', /pergaminho|grimorio|livro/],
  ['escudo', /escudo/],
  ['bota', /bota|sapato/],
  ['racao', /racao|pao|comida|viagem/],
  ['amuleto', /amuleto|colar|anel|talisma/],
]

function identificarTipo(descricao: string): TipoDeMercadoria | null {
  const normalizada = descricao
    .normalize('NFD')
    .replace(/\p{Diacritic}/gu, '')
    .toLowerCase()
  return PALAVRAS.find(([, padrao]) => padrao.test(normalizada))?.[0] ?? null
}

const DESENHOS: Record<TipoDeMercadoria, ReactNode> = {
  pocao: (
    <>
      <rect x="26" y="5" width="12" height="6" rx="2" className="acento" />
      <path d="M28 11v10.5C20.5 24.5 15 31 15 39.5 15 49.5 22.6 57 32 57s17-7.5 17-17.5C49 31 43.5 24.5 36 21.5V11" />
      <path d="M17.6 41h28.8c-.9 8-6.8 13-14.4 13s-13.5-5-14.4-13z" className="liquido" />
      <circle cx="27" cy="47" r="1.8" className="bolha" />
      <circle cx="35.5" cy="44.5" r="1.3" className="bolha" />
      <path d="M22 33c1.5-3 3.6-5 6-6" className="brilho" />
    </>
  ),
  lamina: (
    <g transform="rotate(38 32 32)">
      <path d="M32 3.5 37 13v24H27V13z" />
      <path d="M32 11v24" className="fio" />
      <path d="M20 38.5h24" />
      <path d="M29 39v11.5h6V39" className="acento-traco" />
      <circle cx="32" cy="55" r="3.8" className="acento" />
    </g>
  ),
  capa: (
    <>
      <path d="M32 6c-7.5 0-11.5 6-11.5 12.5 0 3 .9 5.4 2.3 7.3L11.6 54.2c-.6 1.6.6 3.3 2.3 3.3h36.2c1.7 0 2.9-1.7 2.3-3.3L41.2 25.8c1.4-1.9 2.3-4.3 2.3-7.3C43.5 12 39.5 6 32 6z" />
      <path d="M24.5 21.5c2.3 3 4.9 4.6 7.5 4.6s5.2-1.6 7.5-4.6" />
      <path d="M32 31v26M23.5 38 19 57M40.5 38 45 57" className="dobra" />
      <circle cx="32" cy="28.5" r="2.6" className="acento" />
    </>
  ),
  lanterna: (
    <>
      <path d="M26 11a6 6 0 0 1 12 0" />
      <path d="M24.5 17h15l-2.5-6h-10z" className="acento-traco" />
      <rect x="21" y="17" width="22" height="29" rx="3" />
      <path d="M26.5 17v29M37.5 17v29" className="dobra" />
      <path d="M32 40.5c-3.2 0-4.8-2.3-4.8-4.8 0-3.3 3.3-5.2 4.8-9.5 1.5 4.3 4.8 6.2 4.8 9.5 0 2.5-1.6 4.8-4.8 4.8z" className="chama" />
      <path d="M19 46h26l-2.5 7h-21z" />
    </>
  ),
  corda: (
    <>
      <ellipse cx="31" cy="37" rx="20" ry="14" />
      <ellipse cx="31" cy="37" rx="13" ry="8.8" />
      <ellipse cx="31" cy="37" rx="6.2" ry="4" className="acento-traco" />
      <path d="M51 37c3.5 4.5 4.5 10.5 1.5 17" />
      <path d="M17 29.5l2.8 2M24.5 24.5l1.4 3M37.5 24.5 36 27.5M45 29.5l-2.8 2M17 44.5l2.8-2M45 44.5l-2.8-2" className="dobra" />
    </>
  ),
  pergaminho: (
    <>
      <path d="M17 15h30v36H17z" />
      <path d="M13 13.5a4.5 4.5 0 0 1 4.5-4.5h29a4.5 4.5 0 0 1 0 9h-29a4.5 4.5 0 0 1-4.5-4.5z" />
      <path d="M13 52.5a4.5 4.5 0 0 0 4.5 4.5h29a4.5 4.5 0 0 0 0-9h-29a4.5 4.5 0 0 0-4.5 4.5z" />
      <circle cx="32" cy="33" r="4.5" className="acento" />
      <path d="M32 22.5v3.5M32 40v3.5M21.5 33H25M39 33h3.5M24.6 25.6l2.4 2.4M37 38l2.4 2.4M24.6 40.4 27 38M37 28l2.4-2.4" className="acento-traco" />
    </>
  ),
  escudo: (
    <>
      <path d="M32 5.5 52 11.5v16.5c0 14.2-8.4 23.5-20 30-11.6-6.5-20-15.8-20-30V11.5z" />
      <path d="M32 5.5v52.5M12 25h40" className="dobra" />
      <circle cx="32" cy="25" r="5.5" className="acento" />
      <circle cx="18" cy="15.5" r="1.4" className="rebite" />
      <circle cx="46" cy="15.5" r="1.4" className="rebite" />
      <circle cx="21" cy="40" r="1.4" className="rebite" />
      <circle cx="43" cy="40" r="1.4" className="rebite" />
    </>
  ),
  bota: (
    <>
      <path d="M22 6h16v27c0 2.6 1.6 4.9 4 5.8l8.6 3.2c3.3 1.2 5.4 4.3 5.4 7.8V55H14v-7l4-6V10a4 4 0 0 1 4-4z" />
      <path d="M14 50h42" className="acento-traco" />
      <path d="M18 13h20" />
      <path d="M26 20h6M26 26h6M26 32h6" className="dobra" />
    </>
  ),
  racao: (
    <>
      <path d="M19 27c-4.5 4-7 9.8-7 15.5C12 51.5 20.5 57 32 57s20-5.5 20-14.5c0-5.7-2.5-11.5-7-15.5" />
      <path d="M19 27c3-2 5.8-3 8.5-3h9c2.7 0 5.5 1 8.5 3" />
      <path d="M26.5 24 22.5 12.5 32 18l9.5-5.5-4 11.5" className="acento-traco" />
      <path d="M22.5 39c3 1.6 6.2 2.4 9.5 2.4s6.5-.8 9.5-2.4M21 47c3.4 2 7.1 3 11 3s7.6-1 11-3" className="dobra" />
    </>
  ),
  amuleto: (
    <>
      <path d="M19.5 5.5c0 10.5 5 17 12.5 19 7.5-2 12.5-8.5 12.5-19" />
      <circle cx="32" cy="41" r="14.5" />
      <circle cx="32" cy="41" r="9.5" className="dobra" />
      <path d="M32 33v16M27 37l5 4 5-4M27 45l5-4 5 4" className="acento-traco" />
      <circle cx="32" cy="26.5" r="2.4" className="acento" />
    </>
  ),
}

/** Sigilo rúnico determinístico: o mesmo id sempre gera o mesmo desenho. */
function Sigilo({ produtoId }: { produtoId: number }) {
  const tracos: string[] = []
  let semente = (produtoId * 2654435761) % 4294967296
  for (let indice = 0; indice < 4; indice += 1) {
    semente = (semente * 1664525 + 1013904223) % 4294967296
    const angulo = (semente / 4294967296) * Math.PI * 2
    const x = 32 + Math.cos(angulo) * 17
    const y = 34 + Math.sin(angulo) * 17
    tracos.push(`M32 34L${x.toFixed(1)} ${y.toFixed(1)}`)
  }

  return (
    <>
      <circle cx="32" cy="34" r="21" />
      <circle cx="32" cy="34" r="15" className="dobra" />
      <path d={tracos.join('')} className="acento-traco" />
      <circle cx="32" cy="34" r="3" className="acento" />
    </>
  )
}

export function IconeMercadoria({ produtoId, descricao, className }: Props) {
  const tipo = identificarTipo(descricao)

  return (
    <svg
      className={className}
      viewBox="0 0 64 64"
      fill="none"
      stroke="currentColor"
      strokeWidth="2.4"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
      focusable="false"
    >
      {tipo ? DESENHOS[tipo] : <Sigilo produtoId={produtoId} />}
    </svg>
  )
}
