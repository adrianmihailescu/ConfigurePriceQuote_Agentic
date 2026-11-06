import { expect, test } from '@playwright/test'

test('salesperson configures a product and gets an agent workflow preview', async ({ page }) => {
  await page.goto('/')

  await expect(page.getByRole('heading', { name: 'Build the right solution' })).toBeVisible()
  await expect(page.getByRole('heading', { name: 'Edge Server R440' }).first()).toBeVisible()

  await page.getByRole('button', { name: /64 GB/ }).click()
  await expect(page.getByText('$5,082.50')).toBeVisible()

  await page.getByRole('textbox', { name: 'Ask the AI team' }).fill('Recommend a balanced configuration')
  await page.getByRole('button', { name: 'Send to agents' }).click()
  await expect(page.getByText(/configuration is ready to review|configuration is valid and ready for a quote/i)).toBeVisible()
  await expect(page.getByText(/Product Agent:/)).toBeVisible()
  await expect(page.getByText(/Pricing Agent:/)).toBeVisible()
})

test('quote form explains persistence requirements when API is offline', async ({ page }) => {
  await page.goto('/')
  await page.getByRole('button', { name: 'Create a quote' }).click()

  await expect(page.getByRole('dialog')).toBeVisible()
  await expect(page.getByText(/API and SQL Server connection is required/i)).toBeVisible()
  await expect(page.getByLabel('Customer name')).toBeVisible()
  await expect(page.getByLabel('Customer email')).toBeVisible()
})
