import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it } from "vitest";
import { fakeApi } from "../test/fakeApi";
import type { Ticket } from "../types";
import { TicketsPage } from "./TicketsPage";

const ticket: Ticket = {
  key: "KAN-7",
  summary: "Login fails",
  description: "After a reset",
  status: "To Do",
  priority: "High",
  issueType: "Bug",
  assignee: null,
  assigneeAccountId: null,
  created: "2026-10-01T09:00:00Z",
  updated: "2026-10-02T10:30:00Z",
  url: "https://example.atlassian.net/browse/KAN-7",
};

const answers = {
  "GET /api/connection/settings": { baseUrl: "", email: "", projectKey: "KAN", hasApiToken: true },
  "GET /api/tickets": { tickets: [ticket], nextPageToken: "page-2" },
  "GET /api/tickets?nextPageToken=page-2": {
    tickets: [{ ...ticket, key: "KAN-6", summary: "Older one" }],
    nextPageToken: null,
  },
  "GET /api/projects/KAN/issue-types": [
    { id: "1", name: "Task" },
    { id: "2", name: "Bug" },
  ],
  "GET /api/priorities": [
    { id: "2", name: "High" },
    { id: "3", name: "Medium" },
  ],
  "GET /api/projects/KAN/assignable-users": [
    { accountId: "acc-1", displayName: "Ana Lee", emailAddress: null, active: true },
  ],
  "GET /api/tickets/KAN-7": ticket,
  "GET /api/tickets/KAN-7/transitions": [{ id: "31", name: "Start", toStatus: "In Progress" }],
  "GET /api/tickets/KAN-7/comments": [{ id: "100", author: "Ana Lee", body: "Seen it", created: "2026-10-03T08:00:00Z" }],
};

describe("TicketsPage", () => {
  it("lists the tickets and loads the next page", async () => {
    fakeApi(answers);
    const user = userEvent.setup();

    render(<TicketsPage />);

    expect(await screen.findByText("Login fails")).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Load more" }));
    expect(await screen.findByText("Older one")).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Load more" })).not.toBeInTheDocument();
  });

  it("searches with the JQL typed in", async () => {
    const calls = fakeApi({ ...answers, "GET /api/tickets?jql=assignee+%3D+currentUser%28%29": { tickets: [], nextPageToken: null } });
    const user = userEvent.setup();
    render(<TicketsPage />);
    await screen.findByText("Login fails");

    await user.type(screen.getByLabelText("JQL"), "assignee = currentUser()");
    await user.click(screen.getByRole("button", { name: "Search" }));

    expect(await screen.findByText("No tickets found.")).toBeInTheDocument();
    expect(calls.map((c) => c.path)).toContain("/api/tickets?jql=assignee+%3D+currentUser%28%29");
  });

  it("creates a ticket with the project's issue type and an assignee", async () => {
    const calls = fakeApi({
      ...answers,
      "POST /api/tickets": { key: "KAN-8", url: "https://example.atlassian.net/browse/KAN-8" },
      "GET /api/tickets/KAN-8": { ...ticket, key: "KAN-8", summary: "Export to CSV" },
      "GET /api/tickets/KAN-8/transitions": [],
      "GET /api/tickets/KAN-8/comments": [],
    });
    const user = userEvent.setup();
    render(<TicketsPage />);
    await screen.findByText("Login fails");

    await user.click(screen.getByRole("button", { name: "New ticket" }));
    const form = screen.getByRole("form", { name: "New ticket" });
    await within(form).findByRole("option", { name: "Ana Lee" });
    await user.type(within(form).getByLabelText("Summary"), "Export to CSV");
    await user.selectOptions(within(form).getByLabelText("Issue type"), "Bug");
    await user.selectOptions(within(form).getByLabelText("Assignee"), "Ana Lee");
    await user.click(within(form).getByRole("button", { name: "Create" }));

    expect(await screen.findByText("Created KAN-8")).toBeInTheDocument();
    expect(calls.find((c) => c.method === "POST")?.body).toEqual({
      summary: "Export to CSV",
      issueType: "Bug",
      assigneeAccountId: "acc-1",
    });
  });

  it("opens a ticket, moves it with a comment and saves only what changed", async () => {
    const calls = fakeApi({
      ...answers,
      "POST /api/tickets/KAN-7/transitions": undefined,
      "PUT /api/tickets/KAN-7": undefined,
    });
    const user = userEvent.setup();
    render(<TicketsPage />);

    await user.click(await screen.findByRole("button", { name: "KAN-7" }));
    const detail = await screen.findByRole("region", { name: "Ticket KAN-7" });
    expect(await within(detail).findByText("Seen it")).toBeInTheDocument();

    await user.type(within(detail).getByLabelText("Comment for the move"), "Picking it up");
    await user.click(within(detail).getByRole("button", { name: "Start" }));
    expect(await screen.findByText("Moved KAN-7 to In Progress")).toBeInTheDocument();

    await user.clear(within(detail).getByLabelText("Summary"));
    await user.type(within(detail).getByLabelText("Summary"), "Login fails after reset");
    await user.click(within(detail).getByRole("button", { name: "Save" }));
    expect(await screen.findByText("Saved KAN-7")).toBeInTheDocument();

    expect(calls.find((c) => c.method === "POST")?.body).toEqual({ transitionId: "31", comment: "Picking it up" });
    expect(calls.find((c) => c.method === "PUT")?.body).toEqual({ summary: "Login fails after reset" });
  });

  it("deletes a ticket only after it is confirmed", async () => {
    const calls = fakeApi({ ...answers, "DELETE /api/tickets/KAN-7": undefined });
    const user = userEvent.setup();
    render(<TicketsPage />);
    await user.click(await screen.findByRole("button", { name: "KAN-7" }));
    const detail = await screen.findByRole("region", { name: "Ticket KAN-7" });

    await user.click(await within(detail).findByRole("button", { name: "Delete ticket" }));
    expect(calls.some((c) => c.method === "DELETE")).toBe(false);
    await user.click(within(detail).getByRole("button", { name: "Delete KAN-7" }));

    expect(await screen.findByText("Deleted KAN-7")).toBeInTheDocument();
    expect(calls.filter((c) => c.method === "DELETE").map((c) => c.path)).toEqual(["/api/tickets/KAN-7"]);
  });
});
