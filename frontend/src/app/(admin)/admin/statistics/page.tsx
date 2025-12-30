"use client";

import { useState } from "react";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/src/components/ui/tabs";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/src/components/ui/select";
import { OverviewTab, DocumentsTab, UsersTab, ModerationTab, ConversationsTab } from "./_components";

const TIME_RANGES = [
  { value: "7", label: "7 ngày" },
  { value: "14", label: "14 ngày" },
  { value: "30", label: "30 ngày" },
  { value: "90", label: "90 ngày" },
];

export default function AdminStatisticsPage() {
  const [days, setDays] = useState(30);

  return (
    <div className="flex flex-col h-full">
      {/* Header */}
      <div className="flex items-center justify-between border-b px-6 py-4">
        <div>
          <h1 className="text-2xl font-bold">Thống kê</h1>
          <p className="text-sm text-muted-foreground">
            Xem tổng quan và phân tích dữ liệu hệ thống
          </p>
        </div>
        <Select
          value={days.toString()}
          onValueChange={(value) => setDays(parseInt(value))}
        >
          <SelectTrigger className="w-32">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            {TIME_RANGES.map((range) => (
              <SelectItem key={range.value} value={range.value}>
                {range.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      {/* Content */}
      <div className="flex-1 overflow-auto p-6">
        <Tabs defaultValue="overview" className="space-y-6">
          <TabsList className="grid w-full max-w-2xl grid-cols-5">
            <TabsTrigger value="overview">Tổng quan</TabsTrigger>
            <TabsTrigger value="documents">Tài liệu</TabsTrigger>
            <TabsTrigger value="users">Người dùng</TabsTrigger>
            <TabsTrigger value="moderation">Kiểm duyệt</TabsTrigger>
            <TabsTrigger value="conversations">Hội thoại</TabsTrigger>
          </TabsList>

          <TabsContent value="overview" className="mt-6">
            <OverviewTab days={days} />
          </TabsContent>

          <TabsContent value="documents" className="mt-6">
            <DocumentsTab days={days} />
          </TabsContent>

          <TabsContent value="users" className="mt-6">
            <UsersTab days={days} />
          </TabsContent>

          <TabsContent value="moderation" className="mt-6">
            <ModerationTab days={days} />
          </TabsContent>

          <TabsContent value="conversations" className="mt-6">
            <ConversationsTab days={days} />
          </TabsContent>
        </Tabs>
      </div>
    </div>
  );
}
