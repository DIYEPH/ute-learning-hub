"use client";

import { Button } from "../ui/button";
import { Input } from "../ui/input";
import { Label } from "../ui/label";
import AuthDialog from "./auth-dialog";

interface LoginDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
}
export default function LoginDialog({ open, onOpenChange }: LoginDialogProps) {
    return (
        <AuthDialog open={open} onOpenChange={onOpenChange} title="Đăng nhập">
            <div className="space-y-4">
                <div className="space-y-1">
                    <Label>Email or Tên đăng nhập</Label>
                    <Input placeholder="Email or Tên đăng nhập"></Input>
                </div>
                <div className="space-y-1">
                    <Label>Mật khẩu</Label>
                    <Input type="password" placeholder="Mật khẩu" />
                    <div className="text-right text-sm text-blue-600 cursor-pointer mt-1">
                        Quên mật khẩu?
                    </div>
                </div>
                <Button className="w-full h-11 rounded-full text-base">Đăng nhập</Button>
                <Button className="w-full h-11 rounded-full text-base">Đăng nhập với email trường</Button>
            </div>
        </AuthDialog>
    );
}
