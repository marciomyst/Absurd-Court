import { expect, test } from '@playwright/test';

test('partes protocolam defesas, recebem a sentença mockada e avançam para o próximo caso', async ({ browser }) => {
  const hostContext = await browser.newContext();
  const guestContext = await browser.newContext();
  const hostPage = await hostContext.newPage();
  const guestPage = await guestContext.newPage();

  try {
    await hostPage.goto('/');
    await hostPage.locator('#home-name').fill('Juiz E2E');
    await hostPage.getByRole('button', { name: 'Criar novo julgamento' }).click();

    const roomCode = (await hostPage.locator('.lobby__room-code').textContent())?.trim();
    expect(roomCode).toMatch(/^[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}$/);

    await guestPage.goto('/');
    await guestPage.getByRole('button', { name: 'Participar de um julgamento' }).click();
    await guestPage.getByLabel('Código do processo').fill(roomCode!);
    await guestPage.locator('#join-name').fill('Parte E2E');
    await guestPage.getByRole('button', { name: 'Entrar no julgamento' }).click();

    const hostSettings = hostPage.locator('.lobby__setting-row');
    const guestSettings = guestPage.locator('.lobby__setting-row');
    await expect(hostSettings.nth(0).getByRole('button', { name: '3', exact: true })).toHaveClass(/active/);
    await expect(hostSettings.nth(1).getByRole('button', { name: '15s', exact: true })).toHaveClass(/active/);
    await expect(hostSettings.nth(1).getByRole('button', { name: /60s/ })).toBeDisabled();

    await hostSettings.nth(0).getByRole('button', { name: '5', exact: true }).click();
    await expect(hostSettings.nth(0).getByRole('button', { name: '5', exact: true })).toHaveClass(/active/);
    await expect(guestSettings.nth(0).getByRole('button', { name: '5', exact: true })).toHaveClass(/active/);

    await hostSettings.nth(1).getByRole('button', { name: '30s', exact: true }).click();
    await expect(hostSettings.nth(1).getByRole('button', { name: '30s', exact: true })).toHaveClass(/active/);
    await expect(guestSettings.nth(1).getByRole('button', { name: '30s', exact: true })).toHaveClass(/active/);

    await expect(hostPage.getByRole('button', { name: 'Iniciar julgamento' })).toBeEnabled();
    await hostPage.getByRole('button', { name: 'Iniciar julgamento' }).click();

    await expect(hostPage.getByText('Caso 1 / 5')).toBeVisible();
    await expect(guestPage.getByText('Caso 1 / 5')).toBeVisible();

    await guestPage.reload();
    await expect(guestPage.getByText('Caso 1 / 5')).toBeVisible();
    await expect(guestPage.locator('textarea')).toBeEnabled();

    await hostPage.locator('textarea').fill('Minha defesa E2E do juiz.');
    await hostPage.getByRole('button', { name: 'Enviar defesa' }).click();
    await expect(guestPage.getByText('Caso 1 / 5')).toBeVisible();

    await guestPage.locator('textarea').fill('Minha defesa E2E da parte.');
    await guestPage.getByRole('button', { name: 'Enviar defesa' }).click();

    await expect(hostPage.getByText('A sentença · Caso 1')).toBeVisible();
    await expect(guestPage.getByText('A sentença · Caso 1')).toBeVisible();
    await expect(hostPage.getByText('Juiz E2E')).toBeVisible();
    await expect(guestPage.getByText('Aguardando o juiz avançar…')).toBeVisible();
    await expect(hostPage.getByText('justificativa mais meticulosamente absurda', { exact: false })).toBeVisible();

    await hostPage.getByRole('button', { name: /Próximo caso/ }).click();

    await expect(hostPage.getByText('Caso 2 / 5')).toBeVisible();
    await expect(guestPage.getByText('Caso 2 / 5')).toBeVisible();
  } finally {
    await guestContext.close();
    await hostContext.close();
  }
});
