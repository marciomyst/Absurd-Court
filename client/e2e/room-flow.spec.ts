import { expect, test } from '@playwright/test';

test('juiz cria uma sala e outra parte entra usando o código da sessão', async ({ browser }) => {
  const hostContext = await browser.newContext();
  const hostPage = await hostContext.newPage();

  await hostPage.goto('/');
  await hostPage.locator('#home-name').fill('Juiz E2E');
  await hostPage.getByRole('button', { name: 'Criar novo julgamento' }).click();

  const roomCode = (await hostPage.locator('.lobby__room-code').textContent())?.trim();
  expect(roomCode).toMatch(/^[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}$/);

  const guestContext = await browser.newContext();
  const guestPage = await guestContext.newPage();

    await guestPage.goto('/');
    await guestPage.getByRole('button', { name: 'Participar de um julgamento' }).click();
    const roomCodeInput = guestPage.getByLabel('Código do processo');
    await roomCodeInput.fill('zzzz-!!!!-@@@@');
    await expect(roomCodeInput).toHaveValue('');
    await roomCodeInput.fill(roomCode!.toLowerCase());
    await expect(roomCodeInput).toHaveValue(roomCode!);
    await guestPage.locator('#join-name').fill('Parte E2E');
  await guestPage.getByRole('button', { name: 'Entrar no julgamento' }).click();

  await expect(guestPage.locator('.lobby__room-code')).toHaveText(roomCode!);
  await expect(guestPage.locator('.lobby__player-name')).toContainText(['Juiz E2E', 'Parte E2E']);
  await expect(hostPage.locator('.lobby__player-name')).toContainText(['Juiz E2E', 'Parte E2E']);

  await guestContext.close();
  await hostContext.close();
});

test('juiz pode cancelar a pauta e criar uma nova sala na mesma aba', async ({ page }) => {
  await page.goto('/');
  await page.locator('#home-name').fill('Juiz E2E');
  await page.getByRole('button', { name: 'Criar novo julgamento' }).click();
  await expect(page.locator('.lobby__room-code')).toBeVisible();

  await page.getByRole('button', { name: 'Cancelar e voltar' }).click();
  await expect(page.locator('#home-name')).toBeVisible();

  await page.locator('#home-name').fill('Juiz E2E');
  await page.getByRole('button', { name: 'Criar novo julgamento' }).click();
  await expect(page.locator('.lobby__room-code')).toBeVisible();
});
