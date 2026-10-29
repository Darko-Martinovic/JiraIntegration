import { vi } from "vitest";

type Answer = unknown | ((body: unknown) => unknown);

export interface Call {
  method: string;
  path: string;
  body: unknown;
}

/**
 * Stubs fetch with answers keyed by "METHOD /api/path" (the query is part of the
 * path) and records every call. A key without an answer is a 404 problem.
 */
export function fakeApi(answers: Record<string, Answer>) {
  const calls: Call[] = [];
  vi.stubGlobal(
    "fetch",
    vi.fn((url: string, init?: RequestInit) => {
      const method = init?.method ?? "GET";
      const body = typeof init?.body === "string" ? (JSON.parse(init.body) as unknown) : undefined;
      calls.push({ method, path: url, body });

      const key = `${method} ${url}`;
      if (!(key in answers)) {
        return Promise.resolve(json(404, { title: `No answer for ${key}` }));
      }
      const answer = answers[key];
      const value = typeof answer === "function" ? (answer as (body: unknown) => unknown)(body) : answer;
      if (value instanceof Response) {
        return Promise.resolve(value);
      }
      return Promise.resolve(value === undefined ? new Response(null, { status: 204 }) : json(200, value));
    }),
  );
  return calls;
}

export function json(status: number, value: unknown): Response {
  return new Response(JSON.stringify(value), { status, headers: { "Content-Type": "application/json" } });
}
