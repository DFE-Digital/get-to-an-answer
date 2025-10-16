import {defineConfig, devices} from '@playwright/test';

const feBase = {
    testDir: './src/tests/fe',
    testMatch: '**/*.spec.ts',
    use: {
        baseURL: 'https://www.google.co.uk/',
    },
} as const;

const adminBase = {
    testDir: './src/tests/admin',
    testMatch: '**/*.spec.ts',
    use: {
        baseURL: 'https://www.github.com/',
    },
} as const;

export default defineConfig({
    testDir: './src/tests',
    testMatch: '**/*.spec.ts',
    timeout: 20 * 1000, // 10 seconds
    expect: {
        timeout: 5000, // 5 seconds
    },
    fullyParallel: true,
    //retries: 1,
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
            testDir: './src/tests/api',
            testMatch: '**/*.spec.ts',
            use: {
                //baseURL: 'https://www.api.co.uk/',
                //extraHTTPHeaders: {'Accept': 'application/vnd.github.v3+json',},
            },
        },
        {
            name: 'fe-chromium',
            ...feBase,
            use: {...feBase.use, browserName: 'chromium'}
        },
        // {
        //     name: 'fe-firefox',
        //     ...feBase,
        //     use: {...feBase.use, browserName: 'firefox'}
        // },
        // {
        //     name: 'fe-mobile-chrome',
        //     ...feBase,
        //     use: {
        //         ...feBase.use,
        //         browserName: 'chromium',
        //         ...devices['Pixel 8'],
        //         isMobile: true,
        //         viewport: {width: 412, height: 914},
        //         userAgent: devices['Pixel 8']?.userAgent ?? devices['Pixel 7']?.userAgent
        //     }
        // },
        // {
        //     name: 'fe-mobile-safari',
        //     ...feBase,
        //     use: {
        //         ...feBase.use,
        //         browserName: 'webkit',
        //         ...devices['iPhone 15']
        //     }
        // },

    ],
});

