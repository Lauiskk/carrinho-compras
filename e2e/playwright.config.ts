import { defineConfig, devices } from '@playwright/test'

/**
 * Os testes rodam contra a stack de verdade (nginx servindo o front e repassando /api para a API,
 * que fala com o PostgreSQL) — por isso este projeto vive fora do front-end.
 *
 *   docker compose up --build -d
 *   npm --prefix e2e test
 *
 * Para apontar para outro endereço (ex.: o modo local, com `dotnet run` + `npm run dev`):
 *   E2E_BASE_URL=http://localhost:5173 npm --prefix e2e test
 */
export default defineConfig({
  testDir: './testes',
  fullyParallel: true,
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
