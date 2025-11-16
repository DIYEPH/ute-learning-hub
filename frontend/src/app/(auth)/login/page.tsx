'use client';

import LoginDialog from "@/src/components/auth/login-dialog";
import { useState } from "react";

export default function Page() {
  const [open, setOpen] = useState(false);

  return (
    <>
      <button onClick={() => setOpen(true)}>Login</button>
      <LoginDialog open={open} onOpenChange={setOpen} />
    </>
  );
}