import type { ButtonHTMLAttributes } from 'react'
import styles from './Botao.module.css'

type Props = ButtonHTMLAttributes<HTMLButtonElement> & {
  variante: 'latao' | 'cera' | 'tinta' | 'link'
  largo?: boolean
}

export function Botao({ variante, largo = false, className, type = 'button', ...props }: Props) {
  const classes = [styles.botao, styles[variante], largo ? styles.largo : '', className ?? ''].filter(Boolean).join(' ')
  return <button type={type} className={classes} {...props} />
}
