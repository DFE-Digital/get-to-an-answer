import {test} from "@playwright/test";

test('Validate front-end', async ({page}) => {

    console.log("Running front-end tests.........")
    await page.goto('/')
    await page.waitForTimeout(5000);
});