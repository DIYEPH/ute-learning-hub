"use client";

import { useState, useCallback } from 'react';

export function useDropdown(initialOpen = false) {
  const [open, setOpen] = useState(initialOpen);

  const toggle = useCallback(() => {
    setOpen((prev) => !prev);
  }, []);

  const openDropdown = useCallback(() => {
    setOpen(true);
  }, []);

  const closeDropdown = useCallback(() => {
    setOpen(false);
  }, []);

  return {
    open,
    setOpen,
    toggle,
    openDropdown,
    closeDropdown,
  };
}

