import * as jwt from 'jsonwebtoken';

export class SimpleDate extends Date {
    constructor() {
        super();
    }
    
    public addDate(data: {years?: number, months?: number, days?: number}): SimpleDate {
        this.setFullYear(
            this.getFullYear() + (data.years ?? 0), 
            this.getMonth() + (data.months ?? 0), 
            this.getDate() + (data.days ?? 0));
        return this;
    }
}

export class ClaimTypes {
    private static readonly ClaimTypeNamespace = "http://schemas.microsoft.com/ws/2008/06/identity/claims";

    public static readonly Issuer = `iss`;
    public static readonly Audience = `aud`;
    public static readonly AuthenticationInstant = `${(this.ClaimTypeNamespace)}/authenticationinstant`;
    public static readonly AuthenticationMethod = `${this.ClaimTypeNamespace}/authenticationmethod`;
    public static readonly CookiePath = `${this.ClaimTypeNamespace}/cookiepath`;
    public static readonly DenyOnlyPrimarySid = `${this.ClaimTypeNamespace}/denyonlyprimarysid`;
    public static readonly DenyOnlyPrimaryGroupSid = `${this.ClaimTypeNamespace}/denyonlyprimarygroupsid`;
    public static readonly DenyOnlyWindowsDeviceGroup = `${this.ClaimTypeNamespace}/denyonlywindowsdevicegroup`;
    public static readonly Dsa = `${this.ClaimTypeNamespace}/dsa`;
    public static readonly Expiration = `${this.ClaimTypeNamespace}/expiration`;
    public static readonly Expired = `${this.ClaimTypeNamespace}/expired`;
    public static readonly GroupSid = `${this.ClaimTypeNamespace}/groupsid`;
    public static readonly IsPersistent = `${this.ClaimTypeNamespace}/ispersistent`;
    public static readonly PrimaryGroupSid = `${this.ClaimTypeNamespace}/primarygroupsid`;
    public static readonly PrimarySid = `${this.ClaimTypeNamespace}/primarysid`;
    public static readonly Role = `${this.ClaimTypeNamespace}/role`;
    public static readonly SerialNumber = `${this.ClaimTypeNamespace}/serialnumber`;
    public static readonly UserData = `${this.ClaimTypeNamespace}/userdata`;
    public static readonly Version = `${this.ClaimTypeNamespace}/version`;
    public static readonly WindowsAccountName = `${this.ClaimTypeNamespace}/windowsaccountname`;
    public static readonly WindowsDeviceClaim = `${this.ClaimTypeNamespace}/windowsdeviceclaim`;
    public static readonly WindowsDeviceGroup = `${this.ClaimTypeNamespace}/windowsdevicegroup`;
    public static readonly WindowsUserClaim = `${this.ClaimTypeNamespace}/windowsuserclaim`;
    public static readonly WindowsFqbnVersion = `${this.ClaimTypeNamespace}/windowsfqbnversion`;
    public static readonly WindowsSubAuthority = `${this.ClaimTypeNamespace}/windowssubauthority`;

    public static readonly ClaimType2005Namespace = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims";

    public static readonly Subject = 'sub';
    public static readonly Anonymous = `${this.ClaimType2005Namespace}/anonymous`;
    public static readonly Authentication = `${this.ClaimType2005Namespace}/authentication`;
    public static readonly AuthorizationDecision = `${this.ClaimType2005Namespace}/authorizationdecision`;
    public static readonly Country = `${this.ClaimType2005Namespace}/country`;
    public static readonly DateOfBirth = `${this.ClaimType2005Namespace}/dateofbirth`;
    public static readonly Dns = `${this.ClaimType2005Namespace}/dns`;
    public static readonly DenyOnlySid = `${this.ClaimType2005Namespace}/denyonlysid`;
    public static readonly Email = `${this.ClaimType2005Namespace}/emailaddress`;
    public static readonly Gender = `${this.ClaimType2005Namespace}/gender`;
    public static readonly GivenName = `${this.ClaimType2005Namespace}/givenname`;
    public static readonly Hash = `${this.ClaimType2005Namespace}/hash`;
    public static readonly HomePhone = `${this.ClaimType2005Namespace}/homephone`;
    public static readonly Locality = `${this.ClaimType2005Namespace}/locality`;
    public static readonly MobilePhone = `${this.ClaimType2005Namespace}/mobilephone`;
    public static readonly Name = `${this.ClaimType2005Namespace}/name`;
    public static readonly NameIdentifier = `${this.ClaimType2005Namespace}/nameidentifier`;
    public static readonly OtherPhone = `${this.ClaimType2005Namespace}/otherphone`;
    public static readonly PostalCode = `${this.ClaimType2005Namespace}/postalcode`;
    public static readonly Rsa = `${this.ClaimType2005Namespace}/rsa`;
    public static readonly Sid = `${this.ClaimType2005Namespace}/sid`;
    public static readonly Spn = `${this.ClaimType2005Namespace}/spn`;
    public static readonly StateOrProvince = `${this.ClaimType2005Namespace}/stateorprovince`;
    public static readonly StreetAddress = `${this.ClaimType2005Namespace}/streetaddress`;
    public static readonly Surname = `${this.ClaimType2005Namespace}/surname`;
    public static readonly System = `${this.ClaimType2005Namespace}/system`;
    public static readonly Thumbprint = `${this.ClaimType2005Namespace}/thumbprint`;
    public static readonly Upn = `${this.ClaimType2005Namespace}/upn`;
    public static readonly Uri = `${this.ClaimType2005Namespace}/uri`;
    public static readonly Webpage = `${this.ClaimType2005Namespace}/webpage`;
    public static readonly X500DistinguishedName = `${this.ClaimType2005Namespace}/x500distinguishedname`;
    
    private static readonly ClaimType2009Namespace = "http://schemas.xmlsoap.org/ws/2009/09/identity/claims";
    
    public static readonly Actor = `${this.ClaimType2009Namespace}/actor`;
    
}

export class JwtHelper {
    private static readonly secret: string = 'local-test-signing-key-32bytes-minimum!';
    private static readonly defaultIssuer: string = 'http://dfe-issuer';
    private static readonly defaultAudience: string = 'test-audience';

    public static generateToken(customClaims: Record<string, unknown>): string {
        const claims = {
            [ClaimTypes.Issuer]: JwtHelper.defaultIssuer,
            [ClaimTypes.Audience]: JwtHelper.defaultAudience,
            ...customClaims
        };

        return jwt.sign(claims, JwtHelper.secret);
    }

    public static readonly InvalidToken = 'invalid-token';
    
    public static readonly ValidToken = JwtHelper.generateToken({
        [ClaimTypes.Subject]: 'test-subject',
        [ClaimTypes.Name]: 'Test User',
        [ClaimTypes.Email]: 'test@education.gov.uk',
        [ClaimTypes.Role]: ['Admin'],
        [ClaimTypes.Expiration]: new SimpleDate().addDate({days: 1}).toISOString()
    })

    public static readonly ExpiredToken = JwtHelper.generateToken({
        [ClaimTypes.Subject]: 'test-subject',
        [ClaimTypes.Name]: 'Test User',
        [ClaimTypes.Email]: 'test@education.gov.uk',
        [ClaimTypes.Role]: ['Admin'],
        [ClaimTypes.Expiration]: new SimpleDate().addDate({days: -1}).toISOString()
    })

    public static readonly UnauthorizedToken = JwtHelper.generateToken({
        [ClaimTypes.Subject]: 'test-subject',
        [ClaimTypes.Name]: 'Test User',
        [ClaimTypes.Email]: 'other-user@education.gov.uk',
        [ClaimTypes.Role]: ['Admin'],
        [ClaimTypes.Expiration]: new SimpleDate().addDate({days: 1}).toISOString()
    })
}
