import { test, expect } from '@playwright/test';

test.describe('Settings - Configure Times (UC-08)', () => {
  test('should open settings modal and close it', async ({ page }) => {
    const url = `/?t=${Date.now()}-${Math.random()}`;
    await page.goto(url);
    await page.evaluate(() => localStorage.clear());
    await page.reload();

    await page.getByRole('button', { name: 'Configurar tiempos' }).click();

    const modal = page.locator('.modal-backdrop.visible');
    await expect(modal).toBeVisible();
    await expect(page.getByRole('heading', { name: /Configurar Tiempos/i })).toBeVisible();

    await modal.locator('.close-btn').last().click();
    await expect(modal).not.toBeVisible();
  });

  test('should save new timer configuration', async ({ page }) => {
    const url = `/?t=${Date.now()}-${Math.random()}`;
    await page.goto(url);
    await page.evaluate(() => localStorage.clear());
    await page.reload();

    await page.getByRole('button', { name: 'Configurar tiempos' }).click();

    const modal = page.locator('.modal-backdrop.visible');
    await expect(modal).toBeVisible();

    await page.locator('#pomodoro-duration').selectOption('15');
    await page.locator('#short-break').selectOption('3');
    await page.locator('#long-break').selectOption('10');

    await modal.getByRole('button', { name: 'Guardar' }).click();

    await expect(modal).not.toBeVisible();

    const timerText = page.locator('.timer-text-overlay');
    await expect(timerText).toHaveText('15:00');
  });

  test('should disable settings button when timer is running', async ({ page }) => {
    await page.goto(`/?t=${Date.now()}`);

    await page.getByRole('button', { name: 'Iniciar Pomodoro' }).click();

    const settingsButton = page.getByRole('button', { name: 'Configurar tiempos' });
    await expect(settingsButton).toBeDisabled();
  });
});