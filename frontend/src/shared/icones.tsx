type IconeProps = { className?: string }

/** Moeda de ouro cunhada (decorativa: o valor por extenso fica para leitores de tela). */
export function IconeMoeda({ className }: IconeProps) {
  return (
    <svg className={className} viewBox="0 0 24 24" aria-hidden="true" focusable="false">
      <circle cx="12" cy="12" r="10" fill="currentColor" />
      <circle cx="12" cy="12" r="7" fill="none" stroke="rgb(0 0 0 / 0.28)" strokeWidth="1.4" />
      <path d="M12 7.5v9M9.5 9.5 12 7.5l2.5 2" fill="none" stroke="rgb(0 0 0 / 0.32)" strokeWidth="1.4" strokeLinecap="round" />
    </svg>
  )
}

/** Brasão da loja: a balança do mercador. */
export function IconeBalanca({ className }: IconeProps) {
  return (
    <svg className={className} viewBox="0 0 64 64" aria-hidden="true" focusable="false">
      <g fill="none" stroke="currentColor" strokeWidth="3" strokeLinecap="round" strokeLinejoin="round">
        <path d="M32 13v37M22 50h20M13 21h38" />
        <path d="M13 21 7 34a6.5 6.5 0 0 0 13 0zM51 21l-6 13a6.5 6.5 0 0 0 13 0z" />
      </g>
      <circle cx="32" cy="12" r="3" fill="currentColor" />
    </svg>
  )
}
