"use client";

import { useState, useEffect } from 'react';

/**
 * Hook để debounce một giá trị
 * @param value Giá trị cần debounce
 * @param delay Thời gian delay (ms)
 */
export function useDebounce<T>(value: T, delay: number = 300): T {
    const [debouncedValue, setDebouncedValue] = useState<T>(value);

    useEffect(() => {
        const timer = setTimeout(() => {
            setDebouncedValue(value);
        }, delay);

        return () => {
            clearTimeout(timer);
        };
    }, [value, delay]);

    return debouncedValue;
}

/**
 * Hook để kiểm tra tên đã tồn tại (debounced)
 * @param checkFn Function để kiểm tra tên
 * @param delay Thời gian delay (ms)
 */
export function useDuplicateCheck(
    checkFn: (name: string) => Promise<boolean>,
    delay: number = 500
) {
    const [name, setName] = useState('');
    const [isDuplicate, setIsDuplicate] = useState(false);
    const [checking, setChecking] = useState(false);

    const debouncedName = useDebounce(name, delay);

    useEffect(() => {
        if (!debouncedName.trim()) {
            setIsDuplicate(false);
            return;
        }

        const check = async () => {
            setChecking(true);
            try {
                const exists = await checkFn(debouncedName);
                setIsDuplicate(exists);
            } catch {
                setIsDuplicate(false);
            } finally {
                setChecking(false);
            }
        };

        check();
    }, [debouncedName, checkFn]);

    return {
        name,
        setName,
        isDuplicate,
        checking,
        debouncedName,
    };
}
