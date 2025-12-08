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
