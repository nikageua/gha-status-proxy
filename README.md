# GHA Status Proxy

Simple .NET 8 minimal API that exposes the latest GitHub Actions run statuses for any repositories (public or private).

## Features
- Fetches the **latest workflow run** for each configured repository
- Supports **private repositories** via Personal Access Token (PAT)
- Lightweight: single-file API, runs anywhere (Docker, RPi, etc.)

---

## 🚀 Quick Start

### 1. Run via Docker
```bash
docker run -d -e GitHub__Token=ghp_xxx -e GitHub__Repos__0=nikageua/gha-status-proxy -p 8080:8080 ghcr.io/nikageua/gha-status-proxy:latest
```

Then open in your browser or via `curl`:
```bash
http://localhost:8080/api/gha/latest
```

Response example:
```json
{
  "results": [
    {
      "repo": "nikageua/LookBook",
      "status": "completed",
      "conclusion": "success",
      "updatedAt": "2025-10-30T19:40:12Z",
      "url": "https://github.com/nikageua/LookBook/actions/runs/123456789"
    }
  ]
}
```

---

## ⚙️ Configuration

Environment variables:

| Name | Description |
--- | --- |
| `GitHub__Token` | Personal Access Token with `Actions: read` and `Metadata: read` permissions |
| `GitHub__Repos__0`, `GitHub__Repos__1`, ... | Repositories in the form `owner/repo` |

You can also mount the token as a Docker secret at `/run/secrets/github_token`.

### Local config file
- Sample file: `samples/config.json`
- Point the app to a file by setting env var `CONFIG_PATH` to the JSON path. Example:
  - Windows PowerShell: `$env:CONFIG_PATH = 'samples/config.json'`
  - Linux/macOS: `export CONFIG_PATH=samples/config.json`

---

## 🧠 Permissions

Fine-grained token scopes:
- **Repository permissions → Actions:** Read-only
- **Repository permissions → Metadata:** Read-only

Classic token scopes (if used):
- `repo`
- `read:packages`

---

## 🔐 Security

Never hard-code tokens in the image or `homepage` widgets.  
Use environment variables or Docker secrets.

---

## 🧩 Example for Homepage Integration

```yaml
widgets:
  - customapi:
      - title: CI Status
        url: http://gha-proxy:8080/api/gha/latest
        refreshInterval: 60
        mappings:
          - label: gha-status-proxy
            path: results[?(@.repo=='nikageua/gha-status-proxy')].0.conclusion

```

---

## 🧱 Build manually

```bash
dotnet publish -c Release -o out
docker build -t gha-status-proxy .
docker run -p 8080:8080 gha-status-proxy
```

---

## 🪪 License
MIT License © 2025
