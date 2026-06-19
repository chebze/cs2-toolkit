import * as React from "react";
import { cn } from "@/lib/utils";

export const Input = React.forwardRef<HTMLInputElement, React.InputHTMLAttributes<HTMLInputElement>>(
  ({ className, type, ...props }, ref) => (
    <input
      type={type}
      className={cn(
        "flex h-10 w-full rounded-lg border border-[var(--color-border)] bg-[var(--color-muted)] px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-[var(--color-primary)]",
        className
      )}
      ref={ref}
      {...props}
    />
  )
);
Input.displayName = "Input";
