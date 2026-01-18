# AI Assistant Workspace Guide

This document describes how AI assistants can store and organize knowledge discovered during work sessions.

---

## Overview

The `.ai/` folder serves as a dedicated workspace for AI assistants. It provides a structured location to store insights, learnings, and context that can be referenced in future sessions.

---

## Directory Structure

```
.ai/
├── knowledge/              # Long-term persistent knowledge
│   ├── general/            # Cross-project insights
│   │   └── project_overview.md
│   ├── feature/            # Feature-specific documentation
│   │   └── {feature_name}/
│   │       └── *.md
│   └── project/            # Project-specific documentation
│       └── {project_name}/
│           └── *.md
└── memory/                 # Temporary session data
    └── session_{date}/
        └── *.md
```

---

## Storage Categories

### 1. Knowledge (`/.ai/knowledge/`)

**Purpose:** Store persistent, long-term insights that remain valuable across multiple sessions.

| Subfolder | Description | Examples |
|-----------|-------------|----------|
| `general/` | Cross-project patterns and best practices | Architecture decisions, coding standards |
| `feature/{name}/` | Feature-specific implementation details | API integration notes, algorithm explanations |
| `project/{name}/` | Project-specific context and configurations | Environment setup, deployment procedures |

**Best Practices:**
- Use descriptive file names (e.g., `authentication_flow.md`, `database_schema.md`)
- Include a summary section at the top of each document
- Update documents when information changes rather than creating duplicates
- Add timestamps to track when information was last verified

### 2. Memory (`/.ai/memory/`)

**Purpose:** Store temporary, session-specific data that may be discarded after use.

| Use Case | Description |
|----------|-------------|
| Session notes | Temporary context for ongoing work |
| Exploration logs | Notes from investigating issues |
| Draft content | Work-in-progress documentation |

**Retention Policy:**
- Memory files should be considered ephemeral
- Promote valuable insights to `knowledge/` for long-term storage
- Clean up outdated session data periodically

---

## File Naming Conventions

| Pattern | Usage | Example |
|---------|-------|---------|
| `snake_case.md` | Standard documentation | `error_handling.md` |
| `{category}_{topic}.md` | Categorized files | `api_authentication.md` |
| `session_{YYYY-MM-DD}.md` | Session logs | `session_2025-11-30.md` |

---

## Document Template

```markdown
# {Title}

> **Summary:** Brief one-line description of the content.
> 
> **Last Updated:** {YYYY-MM-DD}
> **Category:** {general | feature | project}

## Context

Explain when and why this information is relevant.

## Details

Main content goes here.

## References

- Related files or external links
```

---

## Usage Guidelines

### When to Create Documentation

- ✅ Discovered non-obvious project patterns or conventions
- ✅ Resolved a complex issue with valuable learnings
- ✅ Found important configuration or setup requirements
- ✅ Identified architectural decisions and their rationale

### When NOT to Create Documentation

- ❌ Information already exists in official project docs
- ❌ Trivial or self-explanatory concepts
- ❌ Temporary debugging output (use memory instead)

---

## Integration with Project

This workspace complements the main project documentation:

| Location | Purpose |
|----------|---------|
| `.github/copilot-instructions.md` | Project-wide AI instructions |
| `docs/` | Official project documentation |
| `.ai/knowledge/` | AI-discovered insights and learnings |
| `.ai/memory/` | Temporary session data |

---

## Maintenance

1. **Review periodically:** Archive or delete outdated information
2. **Consolidate duplicates:** Merge similar documents into comprehensive guides
3. **Validate accuracy:** Update documents when project changes occur
4. **Promote valuable insights:** Move proven patterns from `memory/` to `knowledge/`
