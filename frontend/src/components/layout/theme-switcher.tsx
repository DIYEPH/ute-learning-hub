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
        <Button variant="ghost" size="sm" className="group h-9 w-9 rounded-full p-0 hover:bg-muted">
          {resolvedTheme === 'dark' ? (
            <Moon size={18} className="text-muted-foreground transition-colors group-hover:text-foreground" />
          ) : (
            <Sun size={18} className="text-muted-foreground transition-colors group-hover:text-foreground" />
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


