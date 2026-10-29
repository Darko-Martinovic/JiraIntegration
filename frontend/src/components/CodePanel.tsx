import Prism from "prismjs";
import "prismjs/components/prism-csharp";
import "prismjs/themes/prism-tomorrow.css";
import { Code } from "lucide-react";
import { useMemo, useState } from "react";
import type { SourceFile } from "../code";

/**
 * A button that opens the C# behind the page, one tab per file
 */
export function CodePanel({ files }: Readonly<{ files: SourceFile[] }>) {
  const [open, setOpen] = useState(false);
  const [selected, setSelected] = useState(0);
  const [copied, setCopied] = useState(false);
  const file = files[selected];

  // Prism escapes the source before it adds its markup
  const html = useMemo(() => Prism.highlight(file.code, Prism.languages.csharp, "csharp"), [file]);

  const copy = async () => {
    await navigator.clipboard.writeText(file.code);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  return (
    <section className="mt-8">
      <button type="button" className="button-secondary" onClick={() => setOpen(!open)} aria-expanded={open}>
        <Code className="mr-2 h-4 w-4" aria-hidden />
        {open ? "Hide the C# code" : "Show the C# code"}
      </button>
      {open && (
        <div className="mt-4 overflow-hidden rounded-lg border border-gray-200 bg-white">
          <div className="flex flex-wrap items-center gap-1 border-b border-gray-200 bg-gray-50 px-2 py-2" role="tablist">
            {files.map((f, index) => (
              <button
                key={f.name}
                type="button"
                role="tab"
                aria-selected={index === selected}
                onClick={() => setSelected(index)}
                className={`rounded px-3 py-1 text-sm ${
                  index === selected ? "bg-white font-medium text-blue-700 shadow-sm" : "text-gray-600 hover:text-gray-900"
                }`}
              >
                {f.name}
              </button>
            ))}
            <button type="button" className="ml-auto px-3 py-1 text-sm text-gray-600 hover:text-gray-900" onClick={copy}>
              {copied ? "Copied" : "Copy"}
            </button>
          </div>
          <pre className="language-csharp m-0 max-h-[32rem] overflow-auto text-sm">
            <code className="language-csharp" dangerouslySetInnerHTML={{ __html: html }} />
          </pre>
        </div>
      )}
    </section>
  );
}
