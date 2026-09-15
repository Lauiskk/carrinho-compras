import { setupServer } from 'msw/node'

/** Servidor de API falso para os testes: cada teste registra as respostas de que precisa. */
export const servidor = setupServer()
