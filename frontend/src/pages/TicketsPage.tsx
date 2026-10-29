import { ExternalLink, Plus, Search } from "lucide-react";
import { useCallback, useEffect, useState, type FormEvent } from "react";
import { api, errorText } from "../api";
import { ticketCode } from "../code";
import { CodePanel } from "../components/CodePanel";
import { Notice, PageHeader } from "../components/Notice";
import type { SiteSettings, Ticket } from "../types";
import { formatDate } from "../format";
import { NewTicketForm } from "./NewTicketForm";
import { TicketDetail } from "./TicketDetail";

export function TicketsPage() {
  const [settings, setSettings] = useState<SiteSettings | null>(null);
  const [jql, setJql] = useState("");
  const [search, setSearch] = useState("");
  const [tickets, setTickets] = useState<Ticket[]>([]);
  const [nextPageToken, setNextPageToken] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [message, setMessage] = useState<string | null>(null);
  const [creating, setCreating] = useState(false);
  const [selected, setSelected] = useState<string | null>(null);

  const fetchPage = useCallback((query: string, pageToken?: string) => {
    api
      .tickets(query || undefined, pageToken)
      .then(
        (page) => {
          setTickets((shown) => (pageToken ? [...shown, ...page.tickets] : page.tickets));
          setNextPageToken(page.nextPageToken);
        },
        (e: unknown) => setError(errorText(e)),
      )
      .finally(() => setLoading(false));
  }, []);

  useEffect(() => {
    api.settings().then(setSettings, () => setSettings(null));
    fetchPage("");
  }, [fetchPage]);

  const load = (query: string, pageToken?: string) => {
    setLoading(true);
    setError(null);
    fetchPage(query, pageToken);
  };

  const submitSearch = (event: FormEvent) => {
    event.preventDefault();
    setSearch(jql);
    setSelected(null);
    load(jql);
  };

  const refresh = (text: string) => {
    setMessage(text);
    load(search);
  };

  const projectKey = settings?.projectKey ?? "";

  return (
    <>
      <PageHeader title="Tickets">
        Searched with JQL through Jira's enhanced search, one page at a time.
      </PageHeader>

      <form className="mb-4 flex flex-wrap gap-2" onSubmit={submitSearch}>
        <label htmlFor="jql" className="sr-only">
          JQL
        </label>
        <input
          id="jql"
          className="input min-w-64 flex-1 font-mono"
          placeholder={projectKey ? `project = "${projectKey}" ORDER BY created DESC` : "project = KEY ORDER BY created DESC"}
          value={jql}
          onChange={(e) => setJql(e.target.value)}
        />
        <button type="submit" className="button-secondary" disabled={loading}>
          <Search className="mr-2 h-4 w-4" aria-hidden />
          Search
        </button>
        <button type="button" className="button-primary" onClick={() => setCreating(!creating)} disabled={!projectKey}>
          <Plus className="mr-2 h-4 w-4" aria-hidden />
          New ticket
        </button>
      </form>

      <div className="space-y-3">
        {error && <Notice tone="error">{error}</Notice>}
        {message && <Notice tone="success">{message}</Notice>}
      </div>

      {creating && projectKey && (
        <NewTicketForm
          projectKey={projectKey}
          onCancel={() => setCreating(false)}
          onCreated={(created) => {
            setCreating(false);
            setSelected(created.key);
            refresh(`Created ${created.key}`);
          }}
        />
      )}

      <div className="card mt-4 overflow-x-auto p-0">
        <table className="min-w-full divide-y divide-gray-200 text-sm">
          <thead className="bg-gray-50 text-left text-xs font-medium tracking-wide text-gray-500 uppercase">
            <tr>
              <th className="px-4 py-3">Key</th>
              <th className="px-4 py-3">Summary</th>
              <th className="px-4 py-3">Type</th>
              <th className="px-4 py-3">Status</th>
              <th className="px-4 py-3">Priority</th>
              <th className="px-4 py-3">Assignee</th>
              <th className="px-4 py-3">Updated</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100">
            {tickets.map((ticket) => (
              <tr key={ticket.key} className={ticket.key === selected ? "bg-blue-50" : "hover:bg-gray-50"}>
                <td className="px-4 py-2 whitespace-nowrap">
                  <button type="button" className="link font-medium" onClick={() => setSelected(ticket.key)}>
                    {ticket.key}
                  </button>
                  <a href={ticket.url} target="_blank" rel="noreferrer" className="ml-2 inline-block text-gray-400 hover:text-gray-700">
                    <ExternalLink className="h-3.5 w-3.5" aria-label={`Open ${ticket.key} in Jira`} />
                  </a>
                </td>
                <td className="px-4 py-2">{ticket.summary}</td>
                <td className="px-4 py-2">{ticket.issueType}</td>
                <td className="px-4 py-2 whitespace-nowrap">{ticket.status}</td>
                <td className="px-4 py-2">{ticket.priority}</td>
                <td className="px-4 py-2 whitespace-nowrap">{ticket.assignee ?? "Unassigned"}</td>
                <td className="px-4 py-2 whitespace-nowrap text-gray-500">{formatDate(ticket.updated)}</td>
              </tr>
            ))}
          </tbody>
        </table>
        {!loading && tickets.length === 0 && !error && <p className="p-4 text-sm text-gray-500">No tickets found.</p>}
        {loading && <p className="p-4 text-sm text-gray-500">Loading</p>}
      </div>

      {nextPageToken && !loading && (
        <button type="button" className="button-secondary mt-3" onClick={() => load(search, nextPageToken)}>
          Load more
        </button>
      )}

      {selected && (
        <TicketDetail
          key={selected}
          ticketKey={selected}
          projectKey={projectKey}
          onChanged={refresh}
          onDeleted={(text) => {
            setSelected(null);
            refresh(text);
          }}
        />
      )}

      <CodePanel files={ticketCode} />
    </>
  );
}
