import * as jwt from 'jsonwebtoken';

export class SimpleDate extends Date {
    constructor() {
        super();
    }

    public addDate(data: { years?: number, months?: number, days?: number, hours?: number, minutes?: number, seconds?: number }): SimpleDate {
        this.setFullYear(
            this.getFullYear() + (data.years ?? 0),
            this.getMonth() + (data.months ?? 0),
            this.getDate() + (data.days ?? 0));
        this.setHours(this.getHours() + (data.hours ?? 0));
        this.setMinutes(this.getMinutes() + (data.minutes ?? 0));
        this.setSeconds(this.getSeconds() + (data.seconds ?? 0));
        return this;
    }

    /**
     * Returns the date as a number of seconds since the epoch.
     * @constructor
     */
    public ToSeconds() {
        return Math.floor(this.getTime() / 1000);
    }
}

export class ClaimTypes {
    public static readonly Id = "http://schemas.microsoft.com/identity/claims/objectidentifier";
    
    private static readonly ClaimTypeNamespace = "http://schemas.microsoft.com/ws/2008/06/identity/claims";

    public static readonly Issuer = `iss`;
    public static readonly Audience = `aud`;
    public static readonly Expiration = `exp`;
    public static readonly Role = `${this.ClaimTypeNamespace}/role`;
    public static readonly Version = `${this.ClaimTypeNamespace}/version`;

    public static readonly ClaimType2005Namespace = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims";

    public static readonly Subject = 'sub';
    public static readonly Authentication = `${this.ClaimType2005Namespace}/authentication`;
    public static readonly Email = `${this.ClaimType2005Namespace}/emailaddress`;
    public static readonly Name = `${this.ClaimType2005Namespace}/name`;
    public static readonly System = `${this.ClaimType2005Namespace}/system`;

    private static readonly ClaimType2009Namespace = "http://schemas.xmlsoap.org/ws/2009/09/identity/claims";

    public static readonly Actor = `${this.ClaimType2009Namespace}/actor`;

}

export class JwtHelper {
    private static readonly secret: string = 'local-test-signing-key-32bytes-minimum!';
    private static readonly defaultIssuer: string = 'http://dfe-issuer';
    private static readonly defaultAudience: string = 'test-audience';

    public static generateToken(customClaims: Record<string, unknown>): string {
        const claims = {
            [ClaimTypes.Id]: customClaims[ClaimTypes.Email],
            [ClaimTypes.Issuer]: JwtHelper.defaultIssuer,
            [ClaimTypes.Audience]: JwtHelper.defaultAudience,
            ...customClaims
        };

        return jwt.sign(claims, JwtHelper.secret);
    }

    public static readonly ValidToken = JwtHelper.generateToken({
        [ClaimTypes.Subject]: 'test-subject',
        [ClaimTypes.Name]: 'Test User',
        [ClaimTypes.Email]: 'test@education.gov.uk',
        [ClaimTypes.Role]: ['Admin'],
        [ClaimTypes.Expiration]: new SimpleDate().addDate({days: 1}).ToSeconds()
    })

    public static readonly ExpiredToken = JwtHelper.generateToken({
        [ClaimTypes.Subject]: 'test-subject',
        [ClaimTypes.Name]: 'Test User',
        [ClaimTypes.Email]: 'test@education.gov.uk',
        [ClaimTypes.Role]: ['Admin'],
        [ClaimTypes.Expiration]: new SimpleDate().addDate({days: -1}).ToSeconds()
    })

    public static readonly UnauthorizedToken = JwtHelper.generateToken({
        [ClaimTypes.Subject]: 'test-subject',
        [ClaimTypes.Name]: 'Test User',
        [ClaimTypes.Email]: 'other-user@education.gov.uk',
        [ClaimTypes.Role]: ['Admin'],
        [ClaimTypes.Expiration]: new SimpleDate().addDate({days: 1}).ToSeconds()
    })

    public static readonly NoRecordsToken = () => JwtHelper.generateToken({
        [ClaimTypes.Subject]: 'test-subject',
        [ClaimTypes.Name]: 'Test User',
        [ClaimTypes.Email]: `other-user${Math.round(Math.random() * 10000000)}@education.gov.uk`,
        [ClaimTypes.Role]: ['Admin'],
        [ClaimTypes.Expiration]: new SimpleDate().addDate({days: 1}).ToSeconds()
    })

    public static readonly InvalidToken = "invalid-token";
}
