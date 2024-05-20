import { Configuration, LogLevel } from "@azure/msal-browser";


export const msalConfig: Configuration = {
    auth: {
        clientId: process.env.REACT_APP_CLIENT_ID ?? "",                    
        authority:  process.env.REACT_APP_AUTHORITY,     
        redirectUri: '/',                                                   
        postLogoutRedirectUri: '/',         
        navigateToLoginRequestUrl: false,       
    },                            
    cache: {
        cacheLocation: 'localStorage',       
        storeAuthStateInCookie: false,          
    },
    system: {
        loggerOptions: {
            loggerCallback: (level, message, containsPii) => {
                if (containsPii) {
                    return;
                }
                switch (level) {
                    case LogLevel.Error:
                        console.error(message);
                        return;
                    case LogLevel.Info:
                        console.info(message);
                        return;
                    case LogLevel.Verbose:
                        console.debug(message);
                        return;
                    case LogLevel.Warning:
                        console.warn(message);
                        return;
                    default:
                        return;
                }
            },
        },
    },
};

export const loginRequest = {
    scopes: [],
};


/**
 * An optional silentRequest object can be used to achieve silent SSO
 * between applications by providing a "login_hint" property.
 */
// export const silentRequest = {
//     scopes: ["openid", "profile"],
//     loginHint: "example@domain.net"
// };


