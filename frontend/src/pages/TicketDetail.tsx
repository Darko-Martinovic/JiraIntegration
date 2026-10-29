import { ExternalLink, Trash2 } from "lucide-react";
import { useCallback, useEffect, useState, type FormEvent } from "react";
import { api, errorText } from "../api";
import { Notice } from "../components/Notice";
import type { Comment, JiraUser, Priority, Ticket, TicketChanges, Transition } from "../types";
import { formatDate } from "../format";

interface Props {
  ticketKey: string;
  projectKey: string;
  onChanged: (message: string) => void;
  onDeleted: (message: string) => void;
}

/**
 * One ticket: its fields to edit, the transitions its workflow allows, and its comments
 */
export function TicketDetail({ ticketKey, projectKey, onChanged, onDeleted }: Readonly<Props>) {
  const [ticket, setTicket] = useState<Ticket | null>(null);
  const [transitions, setTransitions] = useState<Transition[]>([]);
  const [comments, setComments] = useState<Comment[]>([]);
  const [priorities, setPriorities] = useState<Priority[]>([]);
  const [users, setUsers] = useState<JiraUser[]>([]);
  const [summary, setSummary] = useState("");
  const [description, setDescription] = useState("");
  const [priority, setPriority] = useState("");
  const [assignee, setAssignee] = useState("");
  const [transitionComment, setTransitionComment] = useState("");
  const [newComment, setNewComment] = useState("");
  const [confirmingDelete, setConfirmingDelete] = useState(false);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchTicket = useCallback(
    () => Promise.all([api.ticket(ticketKey), api.transitions(ticketKey), api.comments(ticketKey)]),
    [ticketKey],
  );

  const show = useCallback(([found, foundTransitions, foundComments]: [Ticket, Transition[], Comment[]]) => {
    setTicket(found);
    setSummary(found.summary);
    setDescription(found.description ?? "");
    setPriority(found.priority ?? "");
    setAssignee(found.assigneeAccountId ?? "");
    setTransitions(foundTransitions);
    setComments(foundComments);
  }, []);

  const load = async () => show(await fetchTicket());

  useEffect(() => {
    fetchTicket().then(show, (e: unknown) => setError(errorText(e)));
    api.priorities().then(setPriorities, () => setPriorities([]));
    if (projectKey) {
      api.assignableUsers(projectKey).then(setUsers, () => setUsers([]));
    }
  }, [fetchTicket, show, projectKey]);

  const run = async (action: () => Promise<void>) => {
    setBusy(true);
    setError(null);
    try {
      await action();
    } catch (e) {
      setError(errorText(e));
    } finally {
      setBusy(false);
    }
  };

  const save = (event: FormEvent) => {
    event.preventDefault();
    if (!ticket) {
      return;
    }
    // Only what changed is sent, so Jira only checks those fields
    const changes: TicketChanges = {};
    if (summary !== ticket.summary) changes.summary = summary;
    if (description !== (ticket.description ?? "")) changes.description = description;
    if (priority && priority !== ticket.priority) changes.priority = priority;
    if (assignee !== (ticket.assigneeAccountId ?? "")) changes.assigneeAccountId = assignee;

    void run(async () => {
      await api.updateTicket(ticketKey, changes);
      await load();
      onChanged(`Saved ${ticketKey}`);
    });
  };

  const transition = (t: Transition) =>
    void run(async () => {
      await api.transition(ticketKey, t.id, transitionComment || undefined);
      setTransitionComment("");
      await load();
      onChanged(`Moved ${ticketKey} to ${t.toStatus}`);
    });

  const addComment = (event: FormEvent) => {
    event.preventDefault();
    void run(async () => {
      await api.addComment(ticketKey, newComment);
      setNewComment("");
      setComments(await api.comments(ticketKey));
    });
  };

  const deleteComment = (comment: Comment) =>
    void run(async () => {
      await api.deleteComment(ticketKey, comment.id);
      setComments(await api.comments(ticketKey));
    });

  const deleteTicket = () =>
    void run(async () => {
      await api.deleteTicket(ticketKey);
      onDeleted(`Deleted ${ticketKey}`);
    });

  if (!ticket) {
    return <section className="card mt-6">{error ? <Notice tone="error">{error}</Notice> : "Loading"}</section>;
  }

  return (
    <section className="card mt-6 space-y-6" aria-label={`Ticket ${ticketKey}`}>
      <div className="flex flex-wrap items-center justify-between gap-2">
        <h2 className="text-lg font-semibold text-gray-900">
          {ticket.key}: {ticket.summary}
        </h2>
        <a href={ticket.url} target="_blank" rel="noreferrer" className="link inline-flex items-center text-sm">
          Open in Jira
          <ExternalLink className="ml-1 h-3.5 w-3.5" aria-hidden />
        </a>
      </div>
      <p className="text-sm text-gray-600">
        {ticket.issueType}, {ticket.status}, created {formatDate(ticket.created)}
      </p>

      {error && <Notice tone="error">{error}</Notice>}

      <form className="space-y-4" onSubmit={save} aria-label="Edit ticket">
        <div>
          <label className="label" htmlFor="edit-summary">
            Summary
          </label>
          <input id="edit-summary" className="input" required value={summary} onChange={(e) => setSummary(e.target.value)} />
        </div>
        <div>
          <label className="label" htmlFor="edit-description">
            Description
          </label>
          <textarea
            id="edit-description"
            className="input"
            rows={4}
            value={description}
            onChange={(e) => setDescription(e.target.value)}
          />
        </div>
        <div className="grid gap-4 sm:grid-cols-2">
          <div>
            <label className="label" htmlFor="edit-priority">
              Priority
            </label>
            <select id="edit-priority" className="input" value={priority} onChange={(e) => setPriority(e.target.value)}>
              {!ticket.priority && <option value="">None</option>}
              {priorities.map((p) => (
                <option key={p.id}>{p.name}</option>
              ))}
            </select>
          </div>
          <div>
            <label className="label" htmlFor="edit-assignee">
              Assignee
            </label>
            <select id="edit-assignee" className="input" value={assignee} onChange={(e) => setAssignee(e.target.value)}>
              <option value="">Unassigned</option>
              {ticket.assigneeAccountId && !users.some((u) => u.accountId === ticket.assigneeAccountId) && (
                <option value={ticket.assigneeAccountId}>{ticket.assignee}</option>
              )}
              {users.map((u) => (
                <option key={u.accountId} value={u.accountId}>
                  {u.displayName}
                </option>
              ))}
            </select>
          </div>
        </div>
        <button type="submit" className="button-primary" disabled={busy}>
          Save
        </button>
      </form>

      <div>
        <h3 className="mb-2 font-medium text-gray-900">Move to</h3>
        {transitions.length === 0 ? (
          <p className="text-sm text-gray-500">The workflow allows no move from {ticket.status}.</p>
        ) : (
          <div className="space-y-2">
            <input
              aria-label="Comment for the move"
              className="input"
              placeholder="Comment to add with the move (optional)"
              value={transitionComment}
              onChange={(e) => setTransitionComment(e.target.value)}
            />
            <div className="flex flex-wrap gap-2">
              {transitions.map((t) => (
                <button key={t.id} type="button" className="button-secondary" disabled={busy} onClick={() => transition(t)}>
                  {t.name}
                </button>
              ))}
            </div>
          </div>
        )}
      </div>

      <div>
        <h3 className="mb-2 font-medium text-gray-900">Comments</h3>
        {comments.length === 0 && <p className="text-sm text-gray-500">No comments yet.</p>}
        <ul className="space-y-3">
          {comments.map((c) => (
            <li key={c.id} className="rounded-md border border-gray-200 p-3">
              <div className="mb-1 flex items-center justify-between text-xs text-gray-500">
                <span>
                  {c.author}, {formatDate(c.created)}
                </span>
                <button
                  type="button"
                  className="text-gray-400 hover:text-red-700"
                  disabled={busy}
                  onClick={() => deleteComment(c)}
                  aria-label={`Delete the comment by ${c.author}`}
                >
                  <Trash2 className="h-4 w-4" aria-hidden />
                </button>
              </div>
              <p className="text-sm whitespace-pre-wrap text-gray-800">{c.body}</p>
            </li>
          ))}
        </ul>
        <form className="mt-3 space-y-2" onSubmit={addComment}>
          <textarea
            aria-label="New comment"
            className="input"
            rows={3}
            required
            value={newComment}
            onChange={(e) => setNewComment(e.target.value)}
          />
          <button type="submit" className="button-secondary" disabled={busy}>
            Add comment
          </button>
        </form>
      </div>

      <div className="border-t border-gray-200 pt-4">
        {confirmingDelete ? (
          <div className="flex flex-wrap items-center gap-2 text-sm">
            <span>Delete {ticketKey} from Jira? This can't be undone.</span>
            <button type="button" className="button-danger" disabled={busy} onClick={deleteTicket}>
              Delete {ticketKey}
            </button>
            <button type="button" className="button-secondary" onClick={() => setConfirmingDelete(false)}>
              Keep it
            </button>
          </div>
        ) : (
          <button type="button" className="button-danger" onClick={() => setConfirmingDelete(true)}>
            <Trash2 className="mr-2 h-4 w-4" aria-hidden />
            Delete ticket
          </button>
        )}
      </div>
    </section>
  );
}
