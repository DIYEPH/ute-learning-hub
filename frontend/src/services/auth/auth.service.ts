'use client';

import { PublicClientApplication, AuthenticationResult } from '@azure/msal-browser';
import { postApiAuthLogin, postApiAuthLoginMicrosoft, postApiAuthLogout } from '@/src/api/database/sdk.gen';
import { setAccessToken } from '@/src/api/client';
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

// ============ Auth Service ============

/**
 * Login với email/password
 */
export async function login(emailOrUsername: string, password: string): Promise<LoginResponse> {
    const response = await postApiAuthLogin({
        body: { emailOrUsername, password },
    });

    if (!response.data?.accessToken) {
        throw new Error('Login failed: No access token received');
    }

    setAccessToken(response.data.accessToken);
    return response.data;
}

/**
 * Login với Microsoft
 */
export async function loginWithMicrosoft(): Promise<LoginWithMicrosoftResponse> {
    const instance = await getMsalInstance();
    if (!instance) {
        throw new Error('MSAL not available in this environment');
    }

    const loginResponse: AuthenticationResult = await instance.loginPopup({
        scopes: ['openid', 'profile', 'email'],
        prompt: 'select_account',
    });

    if (!loginResponse.idToken) {
        throw new Error('No ID token from Microsoft');
    }

    const response = await postApiAuthLoginMicrosoft({
        body: { idToken: loginResponse.idToken },
    });

    if (!response.data?.accessToken) {
        throw new Error('Login failed: No access token received');
    }

    setAccessToken(response.data.accessToken);
    return response.data;
}

/**
 * Logout
 */
export async function logout(): Promise<void> {
    try {
        await postApiAuthLogout();
    } catch {
        // Ignore logout API errors
    }

    // Clear local token
    setAccessToken(undefined);

    // Clear MSAL cache if available
    const instance = await getMsalInstance();
    if (instance) {
        const accounts = instance.getAllAccounts();
        if (accounts.length > 0) {
            try {
                await instance.logoutPopup({ account: accounts[0] });
            } catch {
                // Ignore MSAL logout errors
            }
        }
    }
}
