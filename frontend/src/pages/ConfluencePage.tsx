import { ExternalLink, Search } from "lucide-react";
import { useEffect, useState, type FormEvent } from "react";
import { api, errorText } from "../api";
import { confluenceCode } from "../code";
import { CodePanel } from "../components/CodePanel";
import { Notice, PageHeader } from "../components/Notice";
import type { Page, SearchHit, Space } from "../types";

export function ConfluencePage() {
  const [spaces, setSpaces] = useState<Space[]>([]);
  const [space, setSpace] = useState<Space | null>(null);
  const [pages, setPages] = useState<Page[]>([]);
  const [page, setPage] = useState<Page | null>(null);
  const [text, setText] = useState("");
  const [hits, setHits] = useState<SearchHit[] | null>(null);
  const [title, setTitle] = useState("");
  const [body, setBody] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [message, setMessage] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  useEffect(() => {
    api.spaces().then(setSpaces, (e: unknown) => setError(errorText(e)));
  }, []);

  const run = async (action: () => Promise<void>) => {
    setBusy(true);
    setError(null);
    setMessage(null);
    try {
      await action();
    } catch (e) {
      setError(errorText(e));
    } finally {
      setBusy(false);
    }
  };

  const openSpace = (chosen: Space) =>
    void run(async () => {
      setSpace(chosen);
      setPage(null);
      setPages(await api.pages(chosen.key));
    });

  const openPage = (id: string) =>
    void run(async () => {
      setPage(await api.page(id));
    });

  const search = (event: FormEvent) => {
    event.preventDefault();
    void run(async () => {
      setHits(await api.searchConfluence(text, space?.key));
    });
  };

  const create = (event: FormEvent) => {
    event.preventDefault();
    if (!space) {
      return;
    }
    void run(async () => {
      const created = await api.createPage({ spaceKey: space.key, title, text: body });
      setTitle("");
      setBody("");
      setPages(await api.pages(space.key));
      setPage(await api.page(created.id));
      setMessage(`Created ${created.title}`);
    });
  };

  return (
    <>
      <PageHeader title="Confluence">
        The same site and API token as Jira. Spaces and pages come from REST API v2, the search from v1's CQL search.
      </PageHeader>

      <div className="mb-4 space-y-3">
        {error && <Notice tone="error">{error}</Notice>}
        {message && <Notice tone="success">{message}</Notice>}
      </div>

      <form className="mb-6 flex flex-wrap gap-2" onSubmit={search}>
        <label htmlFor="confluence-search" className="sr-only">
          Search text
        </label>
        <input
          id="confluence-search"
          className="input min-w-64 flex-1"
          placeholder={space ? `Search the pages of ${space.name}` : "Search every space"}
          required
          value={text}
          onChange={(e) => setText(e.target.value)}
        />
        <button type="submit" className="button-secondary" disabled={busy}>
          <Search className="mr-2 h-4 w-4" aria-hidden />
          Search
        </button>
      </form>

      {hits && (
        <section className="card mb-6" aria-label="Search results">
          <h2 className="mb-3 font-semibold text-gray-900">
            {hits.length === 0 ? "No pages match" : `${String(hits.length)} matching pages`}
          </h2>
          <ul className="space-y-3">
            {hits.map((hit) => (
              <li key={hit.id || hit.url}>
                <a href={hit.url} target="_blank" rel="noreferrer" className="link font-medium">
                  {hit.title}
                </a>
                {hit.space && <span className="ml-2 text-xs text-gray-500">{hit.space}</span>}
                <p className="text-sm text-gray-600">{hit.excerpt}</p>
              </li>
            ))}
          </ul>
        </section>
      )}

      <div className="grid gap-4 md:grid-cols-[16rem_1fr]">
        <section className="card" aria-label="Spaces">
          <h2 className="mb-3 font-semibold text-gray-900">Spaces</h2>
          <ul className="space-y-1">
            {spaces.map((s) => (
              <li key={s.id}>
                <button
                  type="button"
                  onClick={() => openSpace(s)}
                  className={`w-full rounded px-2 py-1 text-left text-sm ${
                    s.id === space?.id ? "bg-blue-50 font-medium text-blue-800" : "hover:bg-gray-50"
                  }`}
                >
                  {s.name} <span className="text-gray-400">{s.key}</span>
                </button>
              </li>
            ))}
          </ul>
        </section>

        <div className="space-y-4">
          {space && (
            <section className="card" aria-label={`Pages of ${space.name}`}>
              <div className="mb-3 flex items-center justify-between">
                <h2 className="font-semibold text-gray-900">{space.name}</h2>
                <a href={space.url} target="_blank" rel="noreferrer" className="link inline-flex items-center text-sm">
                  Open in Confluence
                  <ExternalLink className="ml-1 h-3.5 w-3.5" aria-hidden />
                </a>
              </div>
              {pages.length === 0 && <p className="text-sm text-gray-500">No pages in this space.</p>}
              <ul className="space-y-1">
                {pages.map((p) => (
                  <li key={p.id}>
                    <button type="button" className="link text-sm" onClick={() => openPage(p.id)}>
                      {p.title}
                    </button>
                  </li>
                ))}
              </ul>
            </section>
          )}

          {page && (
            <section className="card" aria-label={`Page ${page.title}`}>
              <div className="mb-3 flex items-center justify-between">
                <h2 className="font-semibold text-gray-900">{page.title}</h2>
                <a href={page.url} target="_blank" rel="noreferrer" className="link inline-flex items-center text-sm">
                  Open in Confluence
                  <ExternalLink className="ml-1 h-3.5 w-3.5" aria-hidden />
                </a>
              </div>
              <p className="mb-2 text-xs text-gray-500">Version {page.version}, body in storage format:</p>
              <pre className="max-h-80 overflow-auto rounded bg-gray-50 p-3 text-xs whitespace-pre-wrap">{page.body}</pre>
            </section>
          )}

          {space && (
            <form className="card space-y-4" onSubmit={create} aria-label="New page">
              <h2 className="font-semibold text-gray-900">New page in {space.name}</h2>
              <div>
                <label className="label" htmlFor="page-title">
                  Title
                </label>
                <input id="page-title" className="input" required value={title} onChange={(e) => setTitle(e.target.value)} />
              </div>
              <div>
                <label className="label" htmlFor="page-text">
                  Text
                </label>
                <textarea id="page-text" className="input" rows={5} value={body} onChange={(e) => setBody(e.target.value)} />
              </div>
              <button type="submit" className="button-primary" disabled={busy}>
                Create page
              </button>
            </form>
          )}

          {!space && <Notice tone="info">Choose a space to see its pages and add one.</Notice>}
        </div>
      </div>

      <CodePanel files={confluenceCode} />
    </>
  );
}
