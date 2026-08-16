import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './e2e',
  fullyParallel: false,
  workers: 1,
  timeout: 60_000,
  expect: { timeout: 10_000 },
  reporter: 'list',
  use: {
    baseURL: 'http://localhost:4200',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
  },
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],
  webServer: [
    {
      command: 'dotnet run --project ../server/src/AbsurdCourt.Api/AbsurdCourt.Api.csproj -- --environment E2E --urls http://localhost:5122',
      url: 'http://localhost:5122/hubs/court',
      timeout: 120_000,
      reuseExistingServer: false,
    },
    {
      command: 'npm.cmd run start -- --host localhost --port 4200',
      url: 'http://localhost:4200',
      timeout: 120_000,
      reuseExistingServer: !process.env.CI,
    },
  ],
});
