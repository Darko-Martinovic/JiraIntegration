import { NavLink, Route, Routes } from "react-router-dom";
import { ConfluencePage } from "./pages/ConfluencePage";
import { ConnectionPage } from "./pages/ConnectionPage";
import { TicketsPage } from "./pages/TicketsPage";

const links = [
  { to: "/", label: "Connection" },
  { to: "/tickets", label: "Tickets" },
  { to: "/confluence", label: "Confluence" },
];

export default function App() {
  return (
    <div className="min-h-screen bg-gray-100">
      <nav className="border-b border-gray-200 bg-white">
        <div className="mx-auto flex h-14 max-w-6xl items-center gap-8 px-4">
          <span className="font-semibold text-gray-900">Jira Integration</span>
          {links.map((link) => (
            <NavLink
              key={link.to}
              to={link.to}
              end
              className={({ isActive }) =>
                `text-sm font-medium ${isActive ? "text-blue-700" : "text-gray-500 hover:text-gray-800"}`
              }
            >
              {link.label}
            </NavLink>
          ))}
        </div>
      </nav>
      <main className="mx-auto max-w-6xl px-4 py-8">
        <Routes>
          <Route path="/" element={<ConnectionPage />} />
          <Route path="/tickets" element={<TicketsPage />} />
          <Route path="/confluence" element={<ConfluencePage />} />
        </Routes>
      </main>
    </div>
  );
}
