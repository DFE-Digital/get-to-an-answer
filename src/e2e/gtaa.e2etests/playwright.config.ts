import * as dotenv from 'dotenv';
dotenv.config({ path: '.env' });

import {defineConfig, devices} from '@playwright/test';

const apiBase = {
    testDir: './src/tests/api',
    testMatch: '**/*.spec.ts',
    use: {
        baseURL: process.env.API_URL || 'http://localhost:5042',
    },
} as const;

const feBase = {
    testDir: './src/tests/fe',
    testMatch: '**/*.spec.ts',
    use: {
        baseURL: process.env.FE_URL || 'https://www.google.co.uk/',
    },
} as const;

const adminBase = {
    testDir: './src/tests/admin',
    testMatch: '**/*.spec.ts',
    use: {
        baseURL: process.env.ADMIN_URL || 'https://www.github.com/',
    },
} as const;

export default defineConfig({
    testDir: './src/tests',
    testMatch: '**/*.spec.ts',
    timeout: 20 * 1000,
    expect: {
        timeout: 5000,
    },
    
    fullyParallel: true,
    retries: 1,
    workers: 4,
    reporter: [['list'], ['html', {open: 'never'}]],
    use: {
        headless: true,
        trace: 'on-first-retry',
        screenshot: 'only-on-failure',
        video: 'retain-on-failure',
        ignoreHTTPSErrors: true,
    },

    projects: [
        {
            name: 'api',
            ...apiBase,
            use: {...apiBase.use},
        },
        {
            name: 'fe-chromium',
            ...feBase,
            use: {...feBase.use, browserName: 'chromium'}
        },
        {
            name: 'fe-edge',
            ...feBase,
            use: {...feBase.use, channel: 'msedge'}
        },
        {
            name: 'fe-firefox',
            ...feBase,
            use: {...feBase.use, browserName: 'firefox'}
        },
        {
            name: 'fe-mobile-chrome',
            ...feBase,
            use: {
                ...feBase.use,
                browserName: 'chromium',
                ...devices['Pixel 8'],
                isMobile: true,
                viewport: {width: 412, height: 914},
                userAgent: devices['Pixel 8']?.userAgent ?? devices['Pixel 7']?.userAgent
            }
        },
        {
            name: 'fe-mobile-safari',
            ...feBase,
            use: {
                ...feBase.use,
                browserName: 'webkit',
                ...devices['iPhone 15']
            }
        },
        {
            name: 'admin-chromium',
            ...adminBase,
            use: {...adminBase.use, browserName: 'chromium'}
        },
        {
            name: 'admin-firefox',
            ...adminBase,
            use: {...adminBase.use, browserName: 'firefox'}
        },
        {
            name: 'admin-edge',
            ...adminBase,
            use: {...adminBase.use, channel: 'msedge'}
        },
        {
            name: 'admin-mobile-chrome',
            ...adminBase,
            use: {
                ...adminBase.use,
                browserName: 'chromium',
                ...devices['Pixel 8'],
                isMobile: true,
                viewport: {width: 412, height: 914},
                userAgent: devices['Pixel 8']?.userAgent ?? devices['Pixel 7']?.userAgent
            }
        },
        {
            name: 'admin-mobile-safari',
            ...adminBase,
            use: {
                ...adminBase.use,
                browserName: 'webkit',
                ...devices['iPhone 15']
            }
        }
    ],
});