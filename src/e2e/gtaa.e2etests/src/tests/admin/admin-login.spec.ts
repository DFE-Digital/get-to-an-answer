import {test} from "@playwright/test";

test('Validate admin site', async ({page}) => {

    console.log("Running admin tests.........")
    await page.goto('/')
    //await page.waitForTimeout(8000);
});