import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it } from "vitest";
import { fakeApi } from "../test/fakeApi";
import { ConfluencePage } from "./ConfluencePage";

const space = { id: "98304", key: "DEV", name: "Development", description: "Team docs", url: "https://x/wiki/spaces/DEV" };
const notes = { id: "557057", title: "Release notes", spaceId: "98304", version: 3, url: "https://x/wiki/p", body: null };

const answers = {
  "GET /api/confluence/spaces": [space],
  "GET /api/confluence/spaces/DEV/pages": [notes],
  "GET /api/confluence/pages/557057": { ...notes, body: "<p>Version 2.1</p>" },
};

describe("ConfluencePage", () => {
  it("opens a space's pages and shows a page's storage body", async () => {
    fakeApi(answers);
    const user = userEvent.setup();

    render(<ConfluencePage />);
    await user.click(await screen.findByRole("button", { name: "Development DEV" }));
    await user.click(await screen.findByRole("button", { name: "Release notes" }));

    const page = await screen.findByRole("region", { name: "Page Release notes" });
    expect(within(page).getByText("<p>Version 2.1</p>")).toBeInTheDocument();
  });

  it("searches inside the chosen space", async () => {
    const calls = fakeApi({
      ...answers,
      "GET /api/confluence/search?text=release&spaceKey=DEV": [
        { id: "557057", title: "Release notes", excerpt: "the release notes", space: "Development", url: "https://x/wiki/p" },
      ],
    });
    const user = userEvent.setup();
    render(<ConfluencePage />);
    await user.click(await screen.findByRole("button", { name: "Development DEV" }));

    await user.type(screen.getByLabelText("Search text"), "release");
    await user.click(screen.getByRole("button", { name: "Search" }));

    const results = await screen.findByRole("region", { name: "Search results" });
    expect(within(results).getByText("1 matching pages")).toBeInTheDocument();
    expect(within(results).getByText("the release notes")).toBeInTheDocument();
    expect(calls.at(-1)?.path).toBe("/api/confluence/search?text=release&spaceKey=DEV");
  });

  it("creates a page in the chosen space", async () => {
    const calls = fakeApi({
      ...answers,
      "POST /api/confluence/pages": notes,
    });
    const user = userEvent.setup();
    render(<ConfluencePage />);
    await user.click(await screen.findByRole("button", { name: "Development DEV" }));

    const form = await screen.findByRole("form", { name: "New page" });
    await user.type(within(form).getByLabelText("Title"), "Release notes");
    await user.type(within(form).getByLabelText("Text"), "Version 2.1");
    await user.click(within(form).getByRole("button", { name: "Create page" }));

    expect(await screen.findByText("Created Release notes")).toBeInTheDocument();
    expect(calls.find((c) => c.method === "POST")?.body).toEqual({ spaceKey: "DEV", title: "Release notes", text: "Version 2.1" });
  });

  it("shows why the spaces could not be read", async () => {
    fakeApi({});

    render(<ConfluencePage />);

    expect(await screen.findByRole("alert")).toHaveTextContent("No answer for GET /api/confluence/spaces");
  });
});
