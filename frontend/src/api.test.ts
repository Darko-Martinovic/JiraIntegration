import { describe, expect, it } from "vitest";
import { api, ApiError } from "./api";
import { fakeApi, json } from "./test/fakeApi";

describe("api", () => {
  it("leaves out empty query values", async () => {
    const calls = fakeApi({ "GET /api/tickets?jql=project+%3D+KAN": { tickets: [], nextPageToken: null } });

    await api.tickets("project = KAN", undefined);

    expect(calls[0].path).toBe("/api/tickets?jql=project+%3D+KAN");
  });

  it("sends a body as JSON and reads a 204 as nothing", async () => {
    const calls = fakeApi({ "POST /api/tickets/KAN-1/transitions": undefined });

    await expect(api.transition("KAN-1", "31", "Done")).resolves.toBeUndefined();

    expect(calls[0].body).toEqual({ transitionId: "31", comment: "Done" });
  });

  it("turns problem details into the error's message", async () => {
    fakeApi({
      "GET /api/tickets/KAN-99": json(404, { title: "Atlassian could not find it", detail: "Issue does not exist" }),
    });

    const error = await api.ticket("KAN-99").catch((e: unknown) => e);

    expect(error).toBeInstanceOf(ApiError);
    expect((error as ApiError).status).toBe(404);
    expect((error as ApiError).message).toBe("Atlassian could not find it: Issue does not exist");
  });

  it("lists validation errors", async () => {
    fakeApi({
      "POST /api/tickets": json(400, {
        title: "One or more validation errors occurred.",
        errors: { Summary: ["The Summary field is required."] },
      }),
    });

    await expect(api.createTicket({ summary: "", issueType: "Task" })).rejects.toThrow(
      "One or more validation errors occurred.: The Summary field is required.",
    );
  });

  it("names the status when the answer is not JSON", async () => {
    fakeApi({ "GET /api/connection": new Response("Bad gateway", { status: 502, statusText: "Bad Gateway" }) });

    await expect(api.connection()).rejects.toThrow("502 Bad Gateway");
  });
});
