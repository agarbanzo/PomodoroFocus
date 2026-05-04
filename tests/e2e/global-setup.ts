import { spawn, execSync } from 'child_process';
import { chromium } from '@playwright/test';
import * as fs from 'fs';
import * as path from 'path';

const PORT = 5294;
const URL = `http://localhost:${PORT}`;
const TIMEOUT_MS = 60000;

function isPortInUse(port: number): boolean {
  try {
    execSync(`netstat -ano | findstr :${port}`, { stdio: 'pipe' });
    return true;
  } catch {
    return false;
  }
}

function waitForUrl(url: string, timeout: number): Promise<void> {
  return new Promise((resolve, reject) => {
    const start = Date.now();
    const check = async () => {
      try {
        const response = await fetch(url);
        if (response.ok) {
          resolve();
          return;
        }
      } catch { }
      if (Date.now() - start > timeout) {
        reject(new Error(`Timeout waiting for ${url}`));
        return;
      }
      setTimeout(check, 1000);
    };
    check();
  });
}

export default async () => {
  const pidFile = path.join(__dirname, '.webapp.pid');

  if (isPortInUse(PORT)) {
    console.log(`Port ${PORT} already in use, assuming app is running.`);
    return;
  }

  console.log('Starting PomodoroFocus.Web on port 5294...');

  const projectRoot = path.join(__dirname, '..', '..', '..');
  const webProject = path.join(projectRoot, 'PomodoroFocus', 'PomodoroFocus.Web');

  const child = spawn('dotnet', ['run', '--project', webProject], {
    detached: true,
    stdio: 'ignore',
    shell: true,
    cwd: projectRoot,
  });

  child.unref();

  fs.writeFileSync(pidFile, String(child.pid));

  console.log(`Waiting for ${URL} to be ready...`);
  await waitForUrl(URL, TIMEOUT_MS);
  console.log('App is ready.');
};