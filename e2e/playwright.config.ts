import { defineConfig, devices } from '@playwright/test'

/**
 * Os testes rodam contra a stack de verdade (nginx servindo o front e repassando /api para a API,
 * que fala com o PostgreSQL) — por isso este projeto vive fora do front-end.
 *
 *   docker compose up --build -d
 *   npm test
 *
 * Para apontar para outro endereço (ex.: o modo local, com `dotnet run` + `npm run dev`):
 *   E2E_BASE_URL=http://localhost:5173 npm test
 */
export default defineConfig({
  testDir: './testes',
  // Com reserva de estoque o catálogo é estado compartilhado: dois cenários ao mesmo tempo disputariam as
  // mesmas unidades. A concorrência de verdade é exercitada dentro do cenário das duas abas.
  fullyParallel: false,
  workers: 1,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 1 : 0,
  reporter: process.env.CI ? [['list'], ['html', { open: 'never' }]] : [['list']],
  use: {
    baseURL: process.env.E2E_BASE_URL ?? 'http://localhost:3000',
    locale: 'pt-BR',
    timezoneId: 'America/Sao_Paulo',
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
  },
  projects: [
    {
      name: 'desktop',
      use: { ...devices['Desktop Chrome'], viewport: { width: 1440, height: 900 } },
      testIgnore: '**/celular.spec.ts',
    },
    {
      name: 'celular',
      use: { ...devices['Pixel 5'] },
      testMatch: '**/celular.spec.ts',
    },
  ],
})
