import { execSync } from 'child_process';
import * as fs from 'fs';
import * as path from 'path';

const PORT = 5294;

function killProcessOnPort(port: number): void {
  try {
    const output = execSync(`netstat -ano | findstr :${port}`, { encoding: 'utf8' });
    const lines = output.trim().split('\n');
    for (const line of lines) {
      const parts = line.trim().split(/\s+/);
      const pidIndex = parts.length - 1;
      const pid = parseInt(parts[pidIndex], 10);
      if (pid && pid > 0) {
        try {
          execSync(`taskkill /PID ${pid} /F`, { stdio: 'ignore' });
          console.log(`Killed process ${pid} on port ${port}`);
        } catch { }
      }
    }
  } catch { }
}

export default async () => {
  const pidFile = path.join(__dirname, '.webapp.pid');

  if (fs.existsSync(pidFile)) {
    const pid = parseInt(fs.readFileSync(pidFile, 'utf8'), 10);
    try {
      execSync(`taskkill /PID ${pid} /F`, { stdio: 'ignore' });
      console.log(`Killed webapp process ${pid}`);
    } catch { }
    fs.unlinkSync(pidFile);
  }

  killProcessOnPort(PORT);
};