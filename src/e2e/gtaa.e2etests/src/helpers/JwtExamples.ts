import { JwtHelper, ClaimTypes, SimpleDate } from "./JwtHelper";

export function generateToken(
    email: string = 'test.user@example.com',
    name: string = 'Test User'
): string {
    const claims: Record<string, unknown> = {
        [ClaimTypes.Subject]: 'test-subject',
        [ClaimTypes.Name]: name,
        [ClaimTypes.Email]: email,
        [ClaimTypes.Role]: ['Admin'],
        [ClaimTypes.Expiration]: new SimpleDate().addDate({years: 3}).ToSeconds()
    };

    return JwtHelper.generateToken(claims);
}

(async () => {
    try {
        const baseUrl = 'http://localhost:5042';
        const token1 = generateToken("test.user1@example.com", "Test User 1");
        console.log(token1);
        const token2 = generateToken("test.user@example.com", "Test User 2");
        console.log(token2);

        /*const res = await fetch(`${baseUrl}/api/questionnaires`, {
            method: 'POST',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                ...(token ? {'Authorization': `Bearer ${token}`} : {})
            },
            body: JSON.stringify({
                title: 'London Survey',
                slug: '',
                description: 'Description of London Survey'
            })
        });

        if (!res.ok) {
            const text = await res.text().catch(() => '');
            throw new Error(`Request failed ${res.status}: ${text}`);
        }
        console.log(await res.json());*/
    } catch (e) {
        // console.error(e);
    }
})();