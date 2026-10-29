import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { fakeApi } from "../test/fakeApi";
import type { ConnectionStatus } from "../types";
import { ConnectionPage } from "./ConnectionPage";

const settings = { baseUrl: "https://example.atlassian.net", email: "dev@example.com", projectKey: "KAN", hasApiToken: true };

describe("ConnectionPage", () => {
  it("shows who Jira signed in and why Confluence failed", async () => {
    const status: ConnectionStatus = {
      settings,
      missingSettings: [],
      jiraConnected: true,
      jiraError: null,
      user: { accountId: "acc-1", displayName: "Dev", emailAddress: "dev@example.com", active: true },
      confluenceConnected: false,
      confluenceError: "403: No Confluence on this site",
    };
    fakeApi({ "GET /api/connection": status });

    render(<ConnectionPage />);

    expect(await screen.findByText("Signed in as Dev (dev@example.com)")).toBeInTheDocument();
    expect(screen.getByRole("alert")).toHaveTextContent("403: No Confluence on this site");
    expect(screen.getByText("https://example.atlassian.net")).toBeInTheDocument();
  });

  it("names the settings still missing from .env", async () => {
    fakeApi({
      "GET /api/connection": {
        settings: { ...settings, baseUrl: "", hasApiToken: false },
        missingSettings: ["JIRA_BASE_URL", "JIRA_API_TOKEN"],
        jiraConnected: false,
        jiraError: null,
        user: null,
        confluenceConnected: false,
        confluenceError: null,
      } satisfies ConnectionStatus,
    });

    render(<ConnectionPage />);

    expect(await screen.findByRole("alert")).toHaveTextContent("Fill in JIRA_BASE_URL, JIRA_API_TOKEN in .env");
  });

  it("says when the API itself is down", async () => {
    fakeApi({});

    render(<ConnectionPage />);

    expect(await screen.findByRole("alert")).toHaveTextContent("The API could not be reached");
  });
});
