import { PublicClientApplication, AccountInfo, AuthenticationResult } from '@azure/msal-browser';
import { postApiAuthLoginMicrosoft } from '@/src/api/database/sdk.gen';
import type { LoginWithMicrosoftResponse } from '@/src/api/database/types.gen';

// MSAL configuration
const getMsalConfig = () => ({
  auth: {
    clientId: process.env.NEXT_PUBLIC_MICROSOFT_CLIENT_ID || '',
    authority: `https://login.microsoftonline.com/${process.env.NEXT_PUBLIC_MICROSOFT_TENANT_ID || 'common'}`,
    redirectUri: typeof window !== 'undefined' ? window.location.origin : '',
  },
  cache: {
    cacheLocation: 'localStorage',
    storeAuthStateInCookie: false,
  },
});

let msalInstance: PublicClientApplication | null = null;
let initializationPromise: Promise<void> | null = null;

const getMsalInstance = async (): Promise<PublicClientApplication | null> => {
  if (typeof window === 'undefined') return null;
  
  if (!msalInstance) {
    msalInstance = new PublicClientApplication(getMsalConfig());
    initializationPromise = msalInstance.initialize();
  }
  
  // Đảm bảo initialize() đã hoàn thành trước khi return
  if (initializationPromise) {
    await initializationPromise;
  }
  
  return msalInstance;
};

/**
 * Login với Microsoft và lấy tokens từ backend
 */
export async function loginWithMicrosoft(): Promise<LoginWithMicrosoftResponse> {
  const instance = await getMsalInstance();
  
  if (!instance) {
    throw new Error('MSAL instance not available. This function must be called in browser environment.');
  }

  try {
    // 1. Login với Microsoft và lấy idToken
    const loginResponse: AuthenticationResult = await instance.loginPopup({
      scopes: ['openid', 'profile', 'email'],
      prompt: 'select_account',
    });

    if (!loginResponse.idToken) {
      throw new Error('No ID token received from Microsoft');
    }

    // 2. Gửi idToken lên backend để authenticate
    const response = await postApiAuthLoginMicrosoft({
      body: {
        idToken: loginResponse.idToken,
      },
    });

    if (!response.data) {
      throw new Error('No response data from server');
    }

    // 3. Lưu tokens vào localStorage
    if (response.data.accessToken) {
      localStorage.setItem('access_token', response.data.accessToken);
    }
    
    if (response.data.refreshToken) {
      localStorage.setItem('refresh_token', response.data.refreshToken);
    }

    return response.data;
  } catch (error) {
    console.error('Microsoft login error:', error);
    throw error;
  }
}

/**
 * Logout khỏi Microsoft
 */
export async function logoutMicrosoft(): Promise<void> {
  const instance = await getMsalInstance();
  
  if (!instance) return;

  try {
    const accounts = instance.getAllAccounts();
    if (accounts.length > 0) {
      await instance.logoutPopup({
        account: accounts[0],
      });
    }
    
    // Clear local storage
    localStorage.removeItem('access_token');
    localStorage.removeItem('refresh_token');
  } catch (error) {
    console.error('Microsoft logout error:', error);
    throw error;
  }
}

/**
 * Lấy account hiện tại từ Microsoft
 */
export async function getMicrosoftAccount(): Promise<AccountInfo | null> {
  const instance = await getMsalInstance();
  if (!instance) return null;

  const accounts = instance.getAllAccounts();
  return accounts.length > 0 ? accounts[0] : null;
}

