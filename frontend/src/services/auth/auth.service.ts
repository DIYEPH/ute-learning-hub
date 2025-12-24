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

// ============ Auth Service ============

/**
 * Login với email/password
 * Backend sets refresh token in httpOnly cookie automatically
 */
export async function login(emailOrUsername: string, password: string): Promise<LoginResponse> {
    try {
        const response = await postApiAuthLogin({
            body: { emailOrUsername, password },
            throwOnError: true,
        });

        const data = (response.data ?? response) as LoginResponse;
        if (!data?.accessToken) {
            throw new Error('Đăng nhập thất bại');
        }

        // Only save access token - refresh token is in httpOnly cookie
        setAccessToken(data.accessToken);
        return data;
    } catch (error: any) {
        const message = error?.response?.data?.message || error?.message || 'Đăng nhập thất bại';
        throw new Error(message);
    }
}

/**
 * Login với Microsoft
 * Backend sets refresh token in httpOnly cookie automatically
 */
export async function loginWithMicrosoft(): Promise<LoginWithMicrosoftResponse> {
    const instance = await getMsalInstance();
    if (!instance) {
        throw new Error('MSAL không khả dụng');
    }

    try {
        const loginResponse: AuthenticationResult = await instance.loginPopup({
            scopes: ['openid', 'profile', 'email'],
            prompt: 'select_account',
        });

        if (!loginResponse.idToken) {
            throw new Error('Không nhận được ID token từ Microsoft');
        }

        const response = await postApiAuthLoginMicrosoft({
            body: { idToken: loginResponse.idToken },
            throwOnError: true,
        });

        const data = (response.data ?? response) as LoginWithMicrosoftResponse;
        if (!data?.accessToken) {
            throw new Error('Đăng nhập thất bại');
        }

        // Only save access token - refresh token is in httpOnly cookie
        setAccessToken(data.accessToken);
        return data;
    } catch (error: any) {
        const message = error?.response?.data?.message || error?.message || 'Đăng nhập thất bại';
        throw new Error(message);
    }
}

/**
 * Logout
 * Backend clears refresh token cookie automatically
 */
export async function logout(): Promise<void> {
    try {
        await postApiAuthLogout();
    } catch {
    }
    clearTokens();
    const instance = await getMsalInstance();
    if (instance) {
        const accounts = instance.getAllAccounts();
        if (accounts.length > 0) {
            try {
                await instance.logoutPopup({ account: accounts[0] });
            } catch {
            }
        }
    }
}
