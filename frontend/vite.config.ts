import tailwindcss from "@tailwindcss/vite";
import react from "@vitejs/plugin-react";
import { defineConfig } from "vitest/config";

// The API runs on its own port (src/JiraIntegration/appsettings.json); the dev
// server passes /api on to it, so the browser only ever talks to one origin.
const api = process.env.API_URL ?? "http://localhost:5000";

export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    port: 5173,
    proxy: { "/api": api },
    // The pages show the C# services they call, read from ../src
    fs: { allow: [".."] },
  },
  test: {
    environment: "jsdom",
    setupFiles: ["./src/test/setup.ts"],
  },
});
