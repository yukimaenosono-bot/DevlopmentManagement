import { clsx, type ClassValue } from "clsx"
import { twMerge } from "tailwind-merge"

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

/** UTC 文字列 → 日本時間の日時表示（"2026/03/08 10:00"） */
export function formatDateTime(utcString: string): string {
  return new Intl.DateTimeFormat("ja-JP", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
    timeZone: "Asia/Tokyo",
  }).format(new Date(utcString))
}

/** 日付文字列 → 日本時間の日付表示（"2026/03/08"） */
export function formatDate(dateString: string): string {
  // DateOnly ("YYYY-MM-DD") はそのまま new Date() するとローカルTZ依存になるため T00:00:00 を補完
  const normalized = dateString.includes("T") ? dateString : `${dateString}T00:00:00`
  return new Intl.DateTimeFormat("ja-JP", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    timeZone: "Asia/Tokyo",
  }).format(new Date(normalized))
}
