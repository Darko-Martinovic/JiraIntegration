import { RefreshCw } from "lucide-react";
import { useCallback, useEffect, useState } from "react";
import { api, errorText } from "../api";
import { connectionCode } from "../code";
import { CodePanel } from "../components/CodePanel";
import { Notice, PageHeader } from "../components/Notice";
import type { ConnectionStatus } from "../types";

export function ConnectionPage() {
  const [status, setStatus] = useState<ConnectionStatus | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [checking, setChecking] = useState(true);

  const fetchStatus = useCallback(() => {
    api
      .connection()
      .then(setStatus, (e: unknown) => setError(errorText(e)))
      .finally(() => setChecking(false));
  }, []);

  useEffect(fetchStatus, [fetchStatus]);

  const check = () => {
    setChecking(true);
    setError(null);
    fetchStatus();
  };

  return (
    <>
      <PageHeader title="Connection">
        Jira and Confluence share one Atlassian site, one account and one API token, all read from .env.
      </PageHeader>

      <div className="mb-4">
        <button type="button" className="button-secondary" onClick={check} disabled={checking}>
          <RefreshCw className={`mr-2 h-4 w-4 ${checking ? "animate-spin" : ""}`} aria-hidden />
          {checking ? "Checking" : "Check again"}
        </button>
      </div>

      {error && <Notice tone="error">The API could not be reached: {error}</Notice>}

      {status && status.missingSettings.length > 0 && (
        <Notice tone="error">
          Fill in {status.missingSettings.join(", ")} in .env (copy .env.example), then restart the API.
        </Notice>
      )}

      {status && status.missingSettings.length === 0 && (
        <div className="grid gap-4 md:grid-cols-2">
          <section className="card">
            <h2 className="mb-3 font-semibold text-gray-900">Jira</h2>
            {status.jiraConnected && status.user ? (
              <Notice tone="success">
                Signed in as {status.user.displayName}
                {status.user.emailAddress ? ` (${status.user.emailAddress})` : ""}
              </Notice>
            ) : (
              <Notice tone="error">{status.jiraError}</Notice>
            )}
          </section>
          <section className="card">
            <h2 className="mb-3 font-semibold text-gray-900">Confluence</h2>
            {status.confluenceConnected ? (
              <Notice tone="success">The spaces can be read</Notice>
            ) : (
              <Notice tone="error">{status.confluenceError}</Notice>
            )}
          </section>
        </div>
      )}

      {status && (
        <section className="card mt-4">
          <h2 className="mb-3 font-semibold text-gray-900">Settings</h2>
          <dl className="grid grid-cols-[max-content_1fr] gap-x-6 gap-y-2 text-sm">
            <dt className="text-gray-500">Site</dt>
            <dd>{status.settings.baseUrl || "not set"}</dd>
            <dt className="text-gray-500">Account</dt>
            <dd>{status.settings.email || "not set"}</dd>
            <dt className="text-gray-500">API token</dt>
            <dd>{status.settings.hasApiToken ? "set" : "not set"}</dd>
            <dt className="text-gray-500">Project</dt>
            <dd>{status.settings.projectKey || "not set"}</dd>
          </dl>
        </section>
      )}

      <CodePanel files={connectionCode} />
    </>
  );
}
