import { AuthenticationContext, adalFetch, withAdalLogin, AdalConfig } from 'react-adal';

export const adalConfig: AdalConfig = {    
    tenant: '',
    clientId: '',
    endpoints: {
        api: 'https://login.microsoftonline.com/organizations/oauth2/v2.0/token',
    },
    postLogoutRedirectUri: window.location.origin,
    cacheLocation: 'localStorage'
};

export const authContext = new AuthenticationContext(adalConfig);
export const getToken = () => authContext.getCachedToken(adalConfig.clientId);
export const adalApiFetch = (fetch:any, url:any, options:any) =>
    adalFetch(authContext, adalConfig!.endpoints!.api, fetch, url, options);

export const withAdalLoginApi = withAdalLogin(authContext, adalConfig!.endpoints!.api);