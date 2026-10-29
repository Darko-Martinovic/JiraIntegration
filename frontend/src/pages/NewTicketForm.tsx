import { useEffect, useState, type FormEvent } from "react";
import { api, errorText } from "../api";
import { Notice } from "../components/Notice";
import type { CreatedTicket, IssueType, JiraUser, Priority } from "../types";

interface Props {
  projectKey: string;
  onCreated: (created: CreatedTicket) => void;
  onCancel: () => void;
}

export function NewTicketForm({ projectKey, onCreated, onCancel }: Readonly<Props>) {
  const [issueTypes, setIssueTypes] = useState<IssueType[]>([]);
  const [priorities, setPriorities] = useState<Priority[]>([]);
  const [users, setUsers] = useState<JiraUser[]>([]);
  const [summary, setSummary] = useState("");
  const [description, setDescription] = useState("");
  const [issueType, setIssueType] = useState("");
  const [priority, setPriority] = useState("");
  const [assignee, setAssignee] = useState("");
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    // The choices come from the project, since issue types and priorities differ per site
    Promise.all([api.issueTypes(projectKey), api.priorities(), api.assignableUsers(projectKey)]).then(
      ([types, foundPriorities, foundUsers]) => {
        setIssueTypes(types);
        setIssueType((current) => current || types[0]?.name || "");
        setPriorities(foundPriorities);
        setUsers(foundUsers);
      },
      (e: unknown) => setError(errorText(e)),
    );
  }, [projectKey]);

  const submit = async (event: FormEvent) => {
    event.preventDefault();
    setSaving(true);
    setError(null);
    try {
      onCreated(
        await api.createTicket({
          summary,
          description: description || undefined,
          issueType,
          priority: priority || undefined,
          assigneeAccountId: assignee || undefined,
        }),
      );
    } catch (e) {
      setError(errorText(e));
      setSaving(false);
    }
  };

  return (
    <form className="card mt-4 space-y-4" onSubmit={(e) => void submit(e)} aria-label="New ticket">
      <h2 className="font-semibold text-gray-900">New ticket in {projectKey}</h2>
      {error && <Notice tone="error">{error}</Notice>}
      <div>
        <label className="label" htmlFor="new-summary">
          Summary
        </label>
        <input id="new-summary" className="input" required value={summary} onChange={(e) => setSummary(e.target.value)} />
      </div>
      <div>
        <label className="label" htmlFor="new-description">
          Description
        </label>
        <textarea
          id="new-description"
          className="input"
          rows={4}
          value={description}
          onChange={(e) => setDescription(e.target.value)}
        />
      </div>
      <div className="grid gap-4 sm:grid-cols-3">
        <div>
          <label className="label" htmlFor="new-type">
            Issue type
          </label>
          <select id="new-type" className="input" value={issueType} onChange={(e) => setIssueType(e.target.value)}>
            {issueTypes.map((t) => (
              <option key={t.id}>{t.name}</option>
            ))}
          </select>
        </div>
        <div>
          <label className="label" htmlFor="new-priority">
            Priority
          </label>
          <select id="new-priority" className="input" value={priority} onChange={(e) => setPriority(e.target.value)}>
            <option value="">Project default</option>
            {priorities.map((p) => (
              <option key={p.id}>{p.name}</option>
            ))}
          </select>
        </div>
        <div>
          <label className="label" htmlFor="new-assignee">
            Assignee
          </label>
          <select id="new-assignee" className="input" value={assignee} onChange={(e) => setAssignee(e.target.value)}>
            <option value="">Unassigned</option>
            {users.map((u) => (
              <option key={u.accountId} value={u.accountId}>
                {u.displayName}
              </option>
            ))}
          </select>
        </div>
      </div>
      <div className="flex gap-2">
        <button type="submit" className="button-primary" disabled={saving || !issueType}>
          {saving ? "Creating" : "Create"}
        </button>
        <button type="button" className="button-secondary" onClick={onCancel}>
          Cancel
        </button>
      </div>
    </form>
  );
}
