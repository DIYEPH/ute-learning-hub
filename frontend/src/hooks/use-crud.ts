"use client";

import { useState, useCallback, useRef } from "react";

export interface PagedResponse<T> {
    items?: T[] | null;
    totalCount?: number;
    page?: number;
    pageSize?: number;
    totalPages?: number;
}

export interface CrudConfig<TItem, TCreate, TUpdate, TQuery> {
    fetchAll(params?: TQuery): Promise<PagedResponse<TItem> | null>;
    fetchById?(id: string): Promise<TItem | null>;
    create(data: TCreate): Promise<TItem | null>;
    update(id: string, data: TUpdate): Promise<TItem | null>;
    delete(id: string): Promise<void>;
    errorMessages?: { fetch?: string; fetchById?: string; create?: string; update?: string; delete?: string };
}

export interface CrudState<TItem> {
    items: TItem[];
    totalCount: number;
    loading: boolean;
    error: string | null;
}

export interface CrudActions<TItem, TCreate, TUpdate, TQuery> {
    fetchItems: (params?: TQuery) => Promise<PagedResponse<TItem> | null>;
    fetchItemById: (id: string) => Promise<TItem | null>;
    createItem: (data: TCreate) => Promise<TItem | null>;
    updateItem: (id: string, data: TUpdate) => Promise<TItem | null>;
    deleteItem: (id: string) => Promise<void>;
    deleteItems: (ids: string[]) => Promise<void>;
    setItems: (items: TItem[]) => void;
    setTotalCount: (count: number) => void;
    clearError: () => void;
}

export function useCrud<TItem, TCreate, TUpdate, TQuery = Record<string, unknown>>(
    config: CrudConfig<TItem, TCreate, TUpdate, TQuery>
): CrudState<TItem> & CrudActions<TItem, TCreate, TUpdate, TQuery> {
    const [items, setItems] = useState<TItem[]>([]);
    const [totalCount, setTotalCount] = useState(0);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const configRef = useRef(config);
    configRef.current = config;

    const defaultMessages = {
        fetch: "Không thể tải danh sách",
        fetchById: "Không thể tải thông tin",
        create: "Không thể tạo mới",
        update: "Không thể cập nhật",
        delete: "Không thể xóa",
    };

    const getErrorMessage = useCallback((err: unknown, defaultMsg: string): string => {
        if (err && typeof err === 'object') {
            const e = err as { response?: { data?: { message?: string } }; message?: string };
            return e.response?.data?.message || e.message || defaultMsg;
        }
        return defaultMsg;
    }, []);

    // Fetch all items
    const fetchItems = useCallback(async (params?: TQuery): Promise<PagedResponse<TItem> | null> => {
        const messages = { ...defaultMessages, ...configRef.current.errorMessages };
        setLoading(true);
        setError(null);
        try {
            const response = await configRef.current.fetchAll(params);
            if (response) {
                setItems(response.items || []);
                setTotalCount(response.totalCount || 0);
            }
            return response;
        } catch (err) {
            setError(getErrorMessage(err, messages.fetch));
            throw err;
        } finally { setLoading(false); }
    }, [getErrorMessage]);

    // Fetch single item
    const fetchItemById = useCallback(async (id: string): Promise<TItem | null> => {
        if (!configRef.current.fetchById) throw new Error("fetchById not configured");
        const messages = { ...defaultMessages, ...configRef.current.errorMessages };
        setLoading(true);
        setError(null);
        try { return await configRef.current.fetchById(id); }
        catch (err) { setError(getErrorMessage(err, messages.fetchById ?? defaultMessages.fetchById)); throw err; }
        finally { setLoading(false); }
    }, [getErrorMessage]);

    // Create item
    const createItem = useCallback(async (data: TCreate): Promise<TItem | null> => {
        const messages = { ...defaultMessages, ...configRef.current.errorMessages };
        setLoading(true);
        setError(null);
        try { return await configRef.current.create(data); }
        catch (err) { setError(getErrorMessage(err, messages.create ?? defaultMessages.create)); throw err; }
        finally { setLoading(false); }
    }, [getErrorMessage]);

    // Update item
    const updateItem = useCallback(async (id: string, data: TUpdate): Promise<TItem | null> => {
        const messages = { ...defaultMessages, ...configRef.current.errorMessages };
        setLoading(true);
        setError(null);
        try { return await configRef.current.update(id, data); }
        catch (err) { setError(getErrorMessage(err, messages.update ?? defaultMessages.update)); throw err; }
        finally { setLoading(false); }
    }, [getErrorMessage]);

    // Delete item
    const deleteItem = useCallback(async (id: string): Promise<void> => {
        const messages = { ...defaultMessages, ...configRef.current.errorMessages };
        setLoading(true);
        setError(null);
        try { await configRef.current.delete(id); }
        catch (err) { setError(getErrorMessage(err, messages.delete ?? defaultMessages.delete)); throw err; }
        finally { setLoading(false); }
    }, [getErrorMessage]);

    // Delete multiple items
    const deleteItems = useCallback(async (ids: string[]): Promise<void> => {
        const messages = { ...defaultMessages, ...configRef.current.errorMessages };
        setLoading(true);
        setError(null);
        try { await Promise.all(ids.map(id => configRef.current.delete(id))); }
        catch (err) { setError(getErrorMessage(err, messages.delete ?? defaultMessages.delete)); throw err; }
        finally { setLoading(false); }
    }, [getErrorMessage]);

    const clearError = useCallback(() => { setError(null); }, []);

    return {
        items, totalCount, loading, error,
        fetchItems, fetchItemById, createItem, updateItem, deleteItem, deleteItems,
        setItems, setTotalCount, clearError,
    };
}