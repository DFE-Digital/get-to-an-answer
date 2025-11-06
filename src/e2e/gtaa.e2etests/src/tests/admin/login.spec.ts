import {test} from "@playwright/test";
import {SignInPage} from "../../pages/admin/SignInPage";

test.describe('Get to an answer Sign in page', () => {
    let signInPage: SignInPage;
    
    test.beforeEach(async ({page}) => {
        signInPage = new SignInPage(page);
        
        await signInPage.navigateTo('/');
        await signInPage.verifyOnSignInPage();
    });

    test('User can sign in with valid credentials', async ({page}) => {
        const username ='test'; //to be created dynamically
        const password ='test'; //to be created dynamically
        
        await signInPage.signIn(username, password);
    });
});