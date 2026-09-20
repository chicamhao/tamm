#!/usr/bin/env python3
"""graphify framework-noise filter.

Drops Unity/C#/Rx.NET framework reference nodes from the extraction JSON and
collapses per-file reference stubs (*_cs_*) onto the real repo class when one
exists -- so cross-file references between the project's own classes survive,
while `Vector2`, `TMP_Text`, `unityengine_*`, `csharp_namespace:*`,
`IDisposable`, ... are removed.

Usage:  python3 graphify_filter.py graphify-out/.graphify_extract.json
Writes  graphify-out/.graphify_extract.json (in place) and prints a summary.
"""
import json
import re
import sys
from collections import Counter
from pathlib import Path


def norm(label: str) -> str:
    return re.sub(r"[^a-z0-9_]", "_", label.lower())


def main() -> None:
    path = Path(sys.argv[1] if len(sys.argv) > 1 else "graphify-out/.graphify_extract.json")
    ext = json.loads(path.read_text())
    nodes, edges, hyperedges = ext["nodes"], ext["edges"], ext.get("hyperedges", [])

    repo = [n for n in nodes if n.get("source_file") and n["source_file"].startswith("Assets/")]
    by_label: dict[str, list[str]] = {}
    for n in repo:
        by_label.setdefault(norm(n["label"]), []).append(n["id"])

    drop: set[str] = set()
    redirect: dict[str, str] = {}
    framework = Counter()
    collapsed = Counter()
    for n in nodes:
        if n.get("source_file"):
            continue  # own code (incl. file nodes) -- always kept
        nid, label = n["id"], n["label"]
        matches = [m for m in by_label.get(norm(label), []) if m != nid]
        if len(matches) == 1:
            redirect[nid] = matches[0]
            collapsed[label] += 1
        else:
            drop.add(nid)
            framework[label] += 1

    new_edges = []
    for e in edges:
        s = redirect.get(e["source"], e["source"])
        t = redirect.get(e["target"], e["target"])
        if s in drop or t in drop or s == t:
            continue
        # `using X;` edges to framework/system targets whose node does not exist:
        # the builder would re-mint them as external stubs. Drop when the target
        # is not a repo node (own-file imports that resolve to a namespace with a
        # source_file are kept).
        if e.get("relation") == "imports" and t not in {n["id"] for n in repo}:
            continue
        new_edges.append(e)

    nodes = [n for n in nodes if n["id"] not in drop and n["id"] not in redirect]
    ext["nodes"] = nodes
    ext["edges"] = new_edges
    ext["hyperedges"] = hyperedges
    path.write_text(json.dumps(ext), encoding="utf-8")

    print(f"nodes: {len(nodes)}  edges: {len(new_edges)}")
    print(f"  dropped   {len(drop):3d} framework nodes: "
          + ", ".join(f"{v}x {k}" for k, v in framework.most_common(12)))
    print(f"  collapsed {len(redirect):3d} refs onto real classes: "
          + ", ".join(f"{v}x {k}" for k, v in collapsed.most_common(12)))


if __name__ == "__main__":
    main()