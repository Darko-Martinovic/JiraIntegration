/** A date and time as the browser's locale writes it; empty when there is none */
export function formatDate(value: string | null): string {
  return value ? new Date(value).toLocaleString(undefined, { dateStyle: "medium", timeStyle: "short" }) : "";
}
