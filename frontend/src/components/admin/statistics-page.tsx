"use client";

import { useTranslations } from 'next-intl';
import { BarChart3 } from 'lucide-react';

export function StatisticsPage() {
  const t = useTranslations('admin.nav');

  return (
    <div className="container mx-auto py-8 px-4">
      <div className="mb-8">
        <div className="flex items-center gap-3 mb-2">
          <BarChart3 size={32} className="text-foreground" />
          <h1 className="text-3xl font-bold text-foreground">
            {t('statistics')}
          </h1>
        </div>
        <p className="text-slate-600 dark:text-slate-400">
          Xem thống kê và phân tích hệ thống
        </p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <div className="bg-white dark:bg-slate-900 rounded-lg border border-slate-200 dark:border-slate-800 p-6">
          <h3 className="text-sm font-medium text-slate-600 dark:text-slate-400 mb-2">
            Tổng số người dùng
          </h3>
          <p className="text-3xl font-bold text-foreground">-</p>
        </div>

        <div className="bg-white dark:bg-slate-900 rounded-lg border border-slate-200 dark:border-slate-800 p-6">
          <h3 className="text-sm font-medium text-slate-600 dark:text-slate-400 mb-2">
            Tổng số tài liệu
          </h3>
          <p className="text-3xl font-bold text-foreground">-</p>
        </div>

        <div className="bg-white dark:bg-slate-900 rounded-lg border border-slate-200 dark:border-slate-800 p-6">
          <h3 className="text-sm font-medium text-slate-600 dark:text-slate-400 mb-2">
            Tổng số môn học
          </h3>
          <p className="text-3xl font-bold text-foreground">-</p>
        </div>

        <div className="bg-white dark:bg-slate-900 rounded-lg border border-slate-200 dark:border-slate-800 p-6">
          <h3 className="text-sm font-medium text-slate-600 dark:text-slate-400 mb-2">
            Tổng số ngành học
          </h3>
          <p className="text-3xl font-bold text-foreground">-</p>
        </div>
      </div>
    </div>
  );
}

