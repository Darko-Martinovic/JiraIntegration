import { CircleAlert, CircleCheck } from "lucide-react";
import type { ReactNode } from "react";

type Tone = "error" | "success" | "info";

const tones: Record<Tone, string> = {
  error: "border-red-200 bg-red-50 text-red-800",
  success: "border-green-200 bg-green-50 text-green-800",
  info: "border-gray-200 bg-gray-50 text-gray-700",
};

/**
 * A message in a box; errors are announced to screen readers
 */
export function Notice({ tone, children }: Readonly<{ tone: Tone; children: ReactNode }>) {
  const Icon = tone === "success" ? CircleCheck : CircleAlert;
  return (
    <div className={`flex items-start gap-2 rounded-md border p-3 text-sm ${tones[tone]}`} role={tone === "error" ? "alert" : "status"}>
      {tone !== "info" && <Icon className="mt-0.5 h-4 w-4 shrink-0" aria-hidden />}
      <div>{children}</div>
    </div>
  );
}

export function PageHeader({ title, children }: Readonly<{ title: string; children?: ReactNode }>) {
  return (
    <header className="mb-6">
      <h1 className="text-2xl font-semibold text-gray-900">{title}</h1>
      {children && <p className="mt-1 text-gray-600">{children}</p>}
    </header>
  );
}
