"use client";

import {
    createContext,
    useContext,
    useEffect,
    useState,
    useCallback,
    useRef,
    ReactNode,
} from "react";
import {
    HubConnection,
    HubConnectionBuilder,
    LogLevel,
    HubConnectionState,
} from "@microsoft/signalr";
import type { MessageDto } from "@/src/api/database/types.gen";

interface MessageDeletedData {
    messageId: string;
    conversationId: string;
}

interface UserStatusData {
    userId: string;
    conversationId: string;
}

interface UserTypingData {
    conversationId: string;
    userId: string;
    isTyping: boolean;
}

type MessageCallback = (message: MessageDto) => void;
type MessageDeletedCallback = (data: MessageDeletedData) => void;
type UserStatusCallback = (data: UserStatusData) => void;
type UserTypingCallback = (data: UserTypingData) => void;

interface SignalRContextValue {
    connection: HubConnection | null;
    connectionState: HubConnectionState;
    isConnected: boolean;
    // Methods
    joinConversation: (conversationId: string) => Promise<void>;
    leaveConversation: (conversationId: string) => Promise<void>;
    sendTyping: (conversationId: string, isTyping: boolean) => Promise<void>;
    // Event subscriptions - returns unsubscribe function
    onMessageReceived: (callback: MessageCallback) => () => void;
    onMessageUpdated: (callback: MessageCallback) => () => void;
    onMessageDeleted: (callback: MessageDeletedCallback) => () => void;
    onMessagePinned: (callback: MessageCallback) => () => void;
    onMessageUnpinned: (callback: MessageCallback) => () => void;
    onUserOnline: (callback: UserStatusCallback) => () => void;
    onUserOffline: (callback: UserStatusCallback) => () => void;
    onUserTyping: (callback: UserTypingCallback) => () => void;
}

const SignalRContext = createContext<SignalRContextValue | null>(null);

export function useSignalR() {
    const context = useContext(SignalRContext);
    if (!context) {
        throw new Error("useSignalR must be used within a SignalRProvider");
    }
    return context;
}

// Optional hook that doesn't throw if not in provider
export function useSignalROptional() {
    return useContext(SignalRContext);
}

interface SignalRProviderProps {
    children: ReactNode;
}

export function SignalRProvider({ children }: SignalRProviderProps) {
    const [connection, setConnection] = useState<HubConnection | null>(null);
    const [connectionState, setConnectionState] = useState<HubConnectionState>(
        HubConnectionState.Disconnected
    );

    // Store callbacks in refs to avoid recreating connection on callback changes
    const callbacksRef = useRef<{
        messageReceived: Set<MessageCallback>;
        messageUpdated: Set<MessageCallback>;
        messageDeleted: Set<MessageDeletedCallback>;
        messagePinned: Set<MessageCallback>;
        messageUnpinned: Set<MessageCallback>;
        userOnline: Set<UserStatusCallback>;
        userOffline: Set<UserStatusCallback>;
        userTyping: Set<UserTypingCallback>;
    }>({
        messageReceived: new Set(),
        messageUpdated: new Set(),
        messageDeleted: new Set(),
        messagePinned: new Set(),
        messageUnpinned: new Set(),
        userOnline: new Set(),
        userOffline: new Set(),
        userTyping: new Set(),
    });

    // Initialize connection
    useEffect(() => {
        const token = localStorage.getItem("access_token");
        if (!token) {
            console.log("[SignalR] No token, skipping connection");
            return;
        }

        const apiUrl = process.env.NEXT_PUBLIC_API_URL || "https://localhost:7080";
        const hubUrl = `${apiUrl}/hubs/chat`;

        console.log("[SignalR] Creating connection to:", hubUrl);

        const newConnection = new HubConnectionBuilder()
            .withUrl(hubUrl, {
                accessTokenFactory: () => token,
            })
            .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
            .configureLogging(LogLevel.Information)
            .build();

        // Set up event handlers
        newConnection.on("MessageReceived", (message: MessageDto) => {
            console.log("[SignalR] MessageReceived:", message);
            callbacksRef.current.messageReceived.forEach((cb) => cb(message));
        });

        newConnection.on("MessageUpdated", (message: MessageDto) => {
            console.log("[SignalR] MessageUpdated:", message);
            callbacksRef.current.messageUpdated.forEach((cb) => cb(message));
        });

        newConnection.on("MessageDeleted", (data: MessageDeletedData) => {
            console.log("[SignalR] MessageDeleted:", data);
            callbacksRef.current.messageDeleted.forEach((cb) => cb(data));
        });

        newConnection.on("MessagePinned", (message: MessageDto) => {
            console.log("[SignalR] MessagePinned:", message);
            callbacksRef.current.messagePinned.forEach((cb) => cb(message));
        });

        newConnection.on("MessageUnpinned", (message: MessageDto) => {
            console.log("[SignalR] MessageUnpinned:", message);
            callbacksRef.current.messageUnpinned.forEach((cb) => cb(message));
        });

        newConnection.on("UserOnline", (data: UserStatusData) => {
            console.log("[SignalR] UserOnline:", data);
            callbacksRef.current.userOnline.forEach((cb) => cb(data));
        });

        newConnection.on("UserOffline", (data: UserStatusData) => {
            console.log("[SignalR] UserOffline:", data);
            callbacksRef.current.userOffline.forEach((cb) => cb(data));
        });

        newConnection.on("UserTyping", (data: UserTypingData) => {
            console.log("[SignalR] UserTyping:", data);
            callbacksRef.current.userTyping.forEach((cb) => cb(data));
        });

        newConnection.on("Error", (error: string) => {
            console.error("[SignalR] Error from server:", error);
        });

        // Connection state change handlers
        newConnection.onreconnecting(() => {
            console.log("[SignalR] Reconnecting...");
            setConnectionState(HubConnectionState.Reconnecting);
        });

        newConnection.onreconnected(() => {
            console.log("[SignalR] Reconnected");
            setConnectionState(HubConnectionState.Connected);
        });

        newConnection.onclose(() => {
            console.log("[SignalR] Connection closed");
            setConnectionState(HubConnectionState.Disconnected);
        });

        // Start connection
        newConnection
            .start()
            .then(() => {
                console.log("[SignalR] Connected successfully");
                setConnectionState(HubConnectionState.Connected);
                setConnection(newConnection);
            })
            .catch((err) => {
                console.error("[SignalR] Connection failed:", err);
                setConnectionState(HubConnectionState.Disconnected);
            });

        // Cleanup
        return () => {
            console.log("[SignalR] Cleaning up connection");
            newConnection.stop();
        };
    }, []); // Only run once on mount

    // Methods
    const joinConversation = useCallback(
        async (conversationId: string) => {
            if (connection?.state === HubConnectionState.Connected) {
                try {
                    await connection.invoke("JoinConversation", conversationId);
                    console.log("[SignalR] Joined conversation:", conversationId);
                } catch (err) {
                    console.error("[SignalR] Failed to join conversation:", err);
                }
            }
        },
        [connection]
    );

    const leaveConversation = useCallback(
        async (conversationId: string) => {
            if (connection?.state === HubConnectionState.Connected) {
                try {
                    await connection.invoke("LeaveConversation", conversationId);
                    console.log("[SignalR] Left conversation:", conversationId);
                } catch (err) {
                    console.error("[SignalR] Failed to leave conversation:", err);
                }
            }
        },
        [connection]
    );

    const sendTyping = useCallback(
        async (conversationId: string, isTyping: boolean) => {
            if (connection?.state === HubConnectionState.Connected) {
                try {
                    await connection.invoke("SendTyping", conversationId, isTyping);
                } catch (err) {
                    console.error("[SignalR] Failed to send typing:", err);
                }
            }
        },
        [connection]
    );

    // Subscription helpers
    const createSubscription = useCallback(
        <T,>(set: Set<T>) =>
            (callback: T): (() => void) => {
                set.add(callback);
                return () => {
                    set.delete(callback);
                };
            },
        []
    );

    const onMessageReceived = useCallback(
        (callback: MessageCallback) => {
            callbacksRef.current.messageReceived.add(callback);
            return () => {
                callbacksRef.current.messageReceived.delete(callback);
            };
        },
        []
    );

    const onMessageUpdated = useCallback(
        (callback: MessageCallback) => {
            callbacksRef.current.messageUpdated.add(callback);
            return () => {
                callbacksRef.current.messageUpdated.delete(callback);
            };
        },
        []
    );

    const onMessageDeleted = useCallback(
        (callback: MessageDeletedCallback) => {
            callbacksRef.current.messageDeleted.add(callback);
            return () => {
                callbacksRef.current.messageDeleted.delete(callback);
            };
        },
        []
    );

    const onMessagePinned = useCallback(
        (callback: MessageCallback) => {
            callbacksRef.current.messagePinned.add(callback);
            return () => {
                callbacksRef.current.messagePinned.delete(callback);
            };
        },
        []
    );

    const onMessageUnpinned = useCallback(
        (callback: MessageCallback) => {
            callbacksRef.current.messageUnpinned.add(callback);
            return () => {
                callbacksRef.current.messageUnpinned.delete(callback);
            };
        },
        []
    );

    const onUserOnline = useCallback(
        (callback: UserStatusCallback) => {
            callbacksRef.current.userOnline.add(callback);
            return () => {
                callbacksRef.current.userOnline.delete(callback);
            };
        },
        []
    );

    const onUserOffline = useCallback(
        (callback: UserStatusCallback) => {
            callbacksRef.current.userOffline.add(callback);
            return () => {
                callbacksRef.current.userOffline.delete(callback);
            };
        },
        []
    );

    const onUserTyping = useCallback(
        (callback: UserTypingCallback) => {
            callbacksRef.current.userTyping.add(callback);
            return () => {
                callbacksRef.current.userTyping.delete(callback);
            };
        },
        []
    );

    const value: SignalRContextValue = {
        connection,
        connectionState,
        isConnected: connectionState === HubConnectionState.Connected,
        joinConversation,
        leaveConversation,
        sendTyping,
        onMessageReceived,
        onMessageUpdated,
        onMessageDeleted,
        onMessagePinned,
        onMessageUnpinned,
        onUserOnline,
        onUserOffline,
        onUserTyping,
    };

    return (
        <SignalRContext.Provider value={value}>{children}</SignalRContext.Provider>
    );
}

