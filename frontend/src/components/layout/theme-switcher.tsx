'use client';

import { useTheme } from '../providers/theme-provider';
import { Moon, Sun, Monitor } from 'lucide-react';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '../ui/dropdown-menu';
import { Button } from '../ui/button';
import { useTranslations } from 'next-intl';

export function ThemeSwitcher() {
  const { theme, setTheme, resolvedTheme } = useTheme();
  const t = useTranslations('common');

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button variant="ghost" size="sm" className="h-9 w-9 rounded-full p-0 hover:bg-slate-100 dark:hover:bg-slate-800">
          {resolvedTheme === 'dark' ? (
            <Moon size={18} className="text-slate-600 dark:text-slate-400" />
          ) : (
            <Sun size={18} className="text-slate-600 dark:text-slate-400" />
          )}
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end">
        <DropdownMenuItem
          onClick={() => setTheme('light')}
          className={theme === 'light' ? 'bg-accent' : ''}
        >
          <Sun size={16} className="mr-2" />
          {t('themeLight') || 'Light'}
        </DropdownMenuItem>
        <DropdownMenuItem
          onClick={() => setTheme('dark')}
          className={theme === 'dark' ? 'bg-accent' : ''}
        >
          <Moon size={16} className="mr-2" />
          {t('themeDark') || 'Dark'}
        </DropdownMenuItem>
        <DropdownMenuItem
          onClick={() => setTheme('system')}
          className={theme === 'system' ? 'bg-accent' : ''}
        >
          <Monitor size={16} className="mr-2" />
          {t('themeSystem') || 'System'}
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}

