import { JwtHelper, ClaimTypes, SimpleDate } from "./JwtHelper";

function generateToken(): string {
    const claims: Record<string, unknown> = {
        [ClaimTypes.Subject]: 'test-subject',
        [ClaimTypes.Name]: 'Test User',
        [ClaimTypes.Email]: 'test.user@example.com',
        [ClaimTypes.Role]: ['Admin'],
        [ClaimTypes.Expiration]: new SimpleDate().addDate({days: 1}).toISOString()
    };

    return JwtHelper.generateToken(claims);
}

(async () => {
    try {
        const baseUrl = 'http://localhost:5042';
        const token = generateToken();

        console.log(token);

        const res = await fetch(`${baseUrl}/api/questionnaires`, {
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
        console.log(await res.json());
    } catch (e) {
        // console.error(e);
    }
})();