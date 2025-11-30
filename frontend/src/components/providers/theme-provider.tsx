"use client";

import { createContext, useContext, useEffect, useState } from "react";

type Theme = "light" | "dark" | "system";

interface ThemeProviderContextType {
  theme: Theme;
  setTheme: (theme: Theme) => void;
  resolvedTheme: "light" | "dark";
}

const ThemeProviderContext = createContext<ThemeProviderContextType | undefined>(undefined);

export function ThemeProvider({
  children,
  defaultTheme = "system",
  storageKey = "theme",
}: {
  children: React.ReactNode;
  defaultTheme?: Theme;
  storageKey?: string;
}) {
  const [theme, setTheme] = useState<Theme>(defaultTheme);
  const [resolvedTheme, setResolvedTheme] = useState<"light" | "dark">("light");
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    // Chỉ chạy ở client side
    if (typeof window === 'undefined') return;

    // Lấy theme từ localStorage hoặc dùng default
    const stored = localStorage.getItem(storageKey) as Theme | null;
    const initialTheme = (stored && ["light", "dark", "system"].includes(stored)) 
      ? stored 
      : defaultTheme;
    
    setTheme(initialTheme);
    setMounted(true);

    // Set initial theme ngay lập tức để tránh flash
    const root = window.document.documentElement;
    let resolved: "light" | "dark";
    
    if (initialTheme === "system") {
      resolved = window.matchMedia("(prefers-color-scheme: dark)").matches
        ? "dark"
        : "light";
    } else {
      resolved = initialTheme;
    }
    
    root.classList.remove("light", "dark");
    root.classList.add(resolved);
    root.setAttribute("data-theme", resolved);
    setResolvedTheme(resolved);
  }, [storageKey, defaultTheme]);

  useEffect(() => {
    if (!mounted || typeof window === 'undefined') return;

    const root = window.document.documentElement;
    root.classList.remove("light", "dark");

    let resolved: "light" | "dark";

    if (theme === "system") {
      const systemTheme = window.matchMedia("(prefers-color-scheme: dark)").matches
        ? "dark"
        : "light";
      resolved = systemTheme;
    } else {
      resolved = theme;
    }

    setResolvedTheme(resolved);
    root.classList.add(resolved);
    root.setAttribute("data-theme", resolved);
  }, [theme, mounted]);

  // Lắng nghe thay đổi system preference
  useEffect(() => {
    if (theme !== "system" || !mounted || typeof window === 'undefined') return;

    const mediaQuery = window.matchMedia("(prefers-color-scheme: dark)");
    const handleChange = () => {
      const root = window.document.documentElement;
      root.classList.remove("light", "dark");
      const resolved = mediaQuery.matches ? "dark" : "light";
      setResolvedTheme(resolved);
      root.classList.add(resolved);
      root.setAttribute("data-theme", resolved);
    };

    mediaQuery.addEventListener("change", handleChange);
    return () => mediaQuery.removeEventListener("change", handleChange);
  }, [theme, mounted]);

  const value = {
    theme,
    setTheme: (newTheme: Theme) => {
      if (typeof window !== 'undefined') {
        localStorage.setItem(storageKey, newTheme);
      }
      setTheme(newTheme);
    },
    resolvedTheme,
  };

  // Luôn provide context, ngay cả khi chưa mounted
  return (
    <ThemeProviderContext.Provider value={value}>
      {children}
    </ThemeProviderContext.Provider>
  );
}

export function useTheme() {
  const context = useContext(ThemeProviderContext);
  if (context === undefined) {
    throw new Error("useTheme must be used within a ThemeProvider");
  }
  return context;
}

