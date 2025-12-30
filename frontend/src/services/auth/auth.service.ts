'use client';

import { PublicClientApplication, AuthenticationResult } from '@azure/msal-browser';
import { postApiAuthLogin, postApiAuthLoginMicrosoft, postApiAuthLogout } from '@/src/api/database/sdk.gen';
import { setAccessToken, clearTokens } from '@/src/api/client';
import type { LoginResponse, LoginWithMicrosoftResponse } from '@/src/api/database/types.gen';

// ============ MSAL Configuration ============

const getMsalConfig = () => ({
    auth: {
        clientId: process.env.NEXT_PUBLIC_MICROSOFT_CLIENT_ID || '',
        authority: `https://login.microsoftonline.com/${process.env.NEXT_PUBLIC_MICROSOFT_TENANT_ID}`,
        redirectUri: typeof window !== 'undefined' ? window.location.origin : '',
    },
    cache: {
        cacheLocation: 'localStorage' as const,
        storeAuthStateInCookie: false,
    },
});

let msalInstance: PublicClientApplication | null = null;
let initPromise: Promise<void> | null = null;

async function getMsalInstance(): Promise<PublicClientApplication | null> {
    if (typeof window === 'undefined') return null;

    if (!msalInstance) {
        msalInstance = new PublicClientApplication(getMsalConfig());
        initPromise = msalInstance.initialize();
    }

    if (initPromise) await initPromise;
    return msalInstance;
}

// ============ Helper ============

function getErrorMessage(response: any, defaultMsg = 'Đăng nhập thất bại'): string | null {
    // Check SDK error property
    const errData = response?.error || response?.response?.data;
    if (errData?.message) return errData.message;
    if (errData?.title) return errData.title;

    // Check AxiosError
    if (response?.isAxiosError || response?.status >= 400) {
        return response?.response?.data?.message || response?.message || defaultMsg;
    }

    return null;
}

// ============ Auth Service ============

export async function login(emailOrUsername: string, password: string): Promise<LoginResponse> {
    const response = await postApiAuthLogin({ body: { emailOrUsername, password } });

    const error = getErrorMessage(response);
    if (error) throw new Error(error);

    const data = response.data as LoginResponse;
    if (!data?.accessToken) throw new Error('Đăng nhập thất bại');

    setAccessToken(data.accessToken);
    return data;
}

export async function loginWithMicrosoft(): Promise<LoginWithMicrosoftResponse> {
    const instance = await getMsalInstance();
    if (!instance) throw new Error('MSAL không khả dụng');

    let idToken: string;
    try {
        const result: AuthenticationResult = await instance.loginPopup({
            scopes: ['openid', 'profile', 'email'],
            prompt: 'select_account',
        });
        if (!result.idToken) throw new Error('Không nhận được ID token');
        idToken = result.idToken;
    } catch (e: any) {
        throw new Error(e?.message || 'Đăng nhập Microsoft thất bại');
    }

    const response = await postApiAuthLoginMicrosoft({ body: { idToken } });

    const error = getErrorMessage(response);
    if (error) throw new Error(error);

    const data = response.data as LoginWithMicrosoftResponse;
    if (!data?.accessToken) throw new Error('Đăng nhập thất bại');

    setAccessToken(data.accessToken);
    return data;
}

export async function logout(): Promise<void> {
    try { await postApiAuthLogout(); } catch { /* ignore */ }

    clearTokens();

    const instance = await getMsalInstance();
    if (instance) {
        const accounts = instance.getAllAccounts();
        if (accounts.length > 0) {
            try { await instance.logoutPopup({ account: accounts[0] }); } catch { /* ignore */ }
        }
    }
}
