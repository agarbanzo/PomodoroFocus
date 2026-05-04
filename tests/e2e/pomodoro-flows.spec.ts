import { test, expect } from '@playwright/test';

test.describe('Pomodoro Timer - Initial Load (UC-07)', () => {
  test('should load page with correct initial state', async ({ page }) => {
    const url = `/?t=${Date.now()}-${Math.random()}`;
    await page.goto(url);
    await page.evaluate(() => localStorage.clear());
    await page.reload();

    await expect(page.getByRole('heading', { name: 'Pomodoro Focus!' })).toBeVisible();

    const timerText = page.locator('.timer-text-overlay');
    await expect(timerText).toBeVisible();
    await expect(timerText).toHaveText('25:00');

    await expect(page.getByRole('button', { name: 'Iniciar Pomodoro' })).toBeVisible();
  });
});

test.describe('Pomodoro Timer - Start, Pause, Resume (UC-01, UC-02)', () => {
  test('should start timer and show pause/cancel buttons', async ({ page }) => {
    await page.goto(`/?t=${Date.now()}`);

    await page.getByRole('button', { name: 'Iniciar Pomodoro' }).click();

    await expect(page.getByRole('button', { name: 'Pausar' })).toBeVisible();
    await expect(page.locator('.timer-controls .btn-danger')).toBeVisible();
  });

  test('should pause and resume timer', async ({ page }) => {
    await page.goto(`/?t=${Date.now()}`);

    await page.getByRole('button', { name: 'Iniciar Pomodoro' }).click();
    await page.waitForTimeout(1500);

    await page.getByRole('button', { name: 'Pausar' }).click();

    await expect(page.getByRole('button', { name: 'Reanudar' })).toBeVisible();

    await page.getByRole('button', { name: 'Reanudar' }).click();

    await expect(page.getByRole('button', { name: 'Pausar' })).toBeVisible();
  });
});

test.describe('Pomodoro Timer - Cancel (UC-03, UC-04)', () => {
  test('should show cancel confirmation modal and discard session', async ({ page }) => {
    await page.goto(`/?t=${Date.now()}`);

    await page.getByRole('button', { name: 'Iniciar Pomodoro' }).click();
    await page.waitForTimeout(1000);
    await page.locator('.timer-controls .btn-danger').click();

    const modal = page.locator('.modal-backdrop.visible');
    await expect(modal).toBeVisible();

    await expect(modal.getByRole('button', { name: 'Sí, marcar como completado' })).toBeVisible();
    await expect(modal.getByRole('button', { name: 'No, descartar sesión' })).toBeVisible();

    await modal.getByRole('button', { name: 'No, descartar sesión' }).click();

    await expect(modal).not.toBeVisible();
    await expect(page.getByRole('button', { name: 'Iniciar Pomodoro' })).toBeVisible();
  });

  test('should mark pomodoro as completed when confirmed', async ({ page }) => {
    await page.goto(`/?t=${Date.now()}`);

    await page.getByRole('button', { name: 'Iniciar Pomodoro' }).click();
    await page.waitForTimeout(1000);
    await page.locator('.timer-controls .btn-danger').click();

    const modal = page.locator('.modal-backdrop.visible');
    await expect(modal).toBeVisible();

    await modal.getByRole('button', { name: 'Sí, marcar como completado' }).click();

    await expect(page.locator('.dot.active')).toHaveCount(1);
  });
});

test.describe('Pomodoro Timer - Break Buttons (UC-05, UC-06)', () => {
  test('should show break buttons after completing a pomodoro', async ({ page }) => {
    await page.goto(`/?t=${Date.now()}`);

    await page.getByRole('button', { name: 'Iniciar Pomodoro' }).click();
    await page.waitForTimeout(1000);
    await page.locator('.timer-controls .btn-danger').click();
    await page.locator('.modal-backdrop.visible').getByRole('button', { name: 'Sí, marcar como completado' }).click();

    await expect(page.getByRole('button', { name: 'Iniciar Descanso Corto' })).toBeVisible();
  });

  test('should start short break', async ({ page }) => {
    await page.goto(`/?t=${Date.now()}`);

    await page.getByRole('button', { name: 'Iniciar Pomodoro' }).click();
    await page.waitForTimeout(1000);
    await page.locator('.timer-controls .btn-danger').click();
    await page.locator('.modal-backdrop.visible').getByRole('button', { name: 'Sí, marcar como completado' }).click();

    await page.getByRole('button', { name: 'Iniciar Descanso Corto' }).click();

    await expect(page.getByRole('button', { name: 'Pausar' })).toBeVisible();
    await expect(page.locator('.timer-controls .btn-danger')).toBeVisible();
  });
});