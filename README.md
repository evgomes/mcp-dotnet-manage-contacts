# .NET MCP Server

A minimal **MCP (Model Context Protocol) HTTP server** implemented in **.NET 9** that exposes two tools to **create** and **list** contacts stored **in memory**. It's designed for tutorials and quick experiments, and includes instructions to test it with **Claude Desktop** via `mcp-remote`.

---

## What's inside

```
ManageContacts.McpServer/
├─ Tools/
│  └─ ContactTools.cs            # MCP tools: create-contact, list-contacts
├─ Properties/
│  └─ launchSettings.json        # Dev profile on http://localhost:5096
├─ appsettings.json
├─ ManageContacts.McpServer.csproj
└─ Program.cs                    # ASP.NET Core host + MCP HTTP transport (/mcp)
```

### Tools

**Namespace:** `ManageContacts.McpServer.Tools.ContactTools`

- `create-contact`

  - **Description:** Creates a new contact and stores it in memory.
  - **Parameters:**

    - `fullName` _(string, required)_ – Full name of the contact.
    - `email` _(string, required)_ – Email address of the contact.

  - **Returns:** `{ fullName, email }`

- `list-contacts`

  - **Description:** Lists contacts from the in-memory store, optionally filtering by name/email.
  - **Parameters:**

    - `searchTerm` _(string, optional)_ – Case-insensitive substring match against name or email.

  - **Returns:** `Array<{ fullName, email }>`

> ⚠️ **Data is in-memory.** Contacts are lost when the process restarts.

---

## Requirements

- **.NET SDK 9.0**
- **Node.js** (for `npx mcp-remote`)
- **Claude Desktop** (to test via MCP)

---

## Running the server

From the solution root (or the project folder), run:

```bash
dotnet run --project ManageContacts.McpServer
```

By default (see `launchSettings.json`), the app listens on:

```
http://localhost:5096
```

The MCP endpoint is:

```
http://localhost:5096/mcp
```

---

## Testing with Claude Desktop

This project is set up to use the **HTTP transport** with Anthropic's **Claude Desktop** via `mcp-remote`.

### 1) Start the MCP server

```bash
dotnet run --project ManageContacts.McpServer
```

Confirm it's running at `http://localhost:5096/mcp`.

### 2) Configure Claude Desktop

Use the config you provided. In **Claude Desktop**:

- Open **Settings → Developer → Edit Config** (or the equivalent config editor in your version).
- Paste the following JSON:

```json
{
  "mcpServers": {
    "manage-contacts": {
      "command": "npx",
      "args": ["-y", "mcp-remote", "http://localhost:5096/mcp"]
    }
  }
}
```

Save the config and **restart Claude Desktop**.

> What this does: Claude will run `npx -y mcp-remote http://localhost:5096/mcp`, which bridges Claude's MCP client to your HTTP MCP server.

### 3) Use the tools in a chat

Open a new chat with Claude and try prompts like:

- "Create a contact named _Ellen Ripley_ with email _[ellen@test.com](mailto:ellen@test.com)_."
- "List all contacts."
- "List contacts containing _ellen_ in name or email."

Claude should decide to call:

- `create-contact` with `{ "fullName": "...", "email": "..." }`
- `list-contacts` with `{ "searchTerm": "..." }` (or no args)

You'll see tool call results in the chat, and your server logs will show incoming MCP requests.

---

## How it works (quick tour)

- **Program.cs**

  ```csharp
  builder.Services
      .AddMcpServer()
      .WithHttpTransport()
      .WithToolsFromAssembly();

  app.MapMcp("/mcp");
  ```

  This registers the MCP server, enables **HTTP transport**, scans the assembly for `[McpServerTool]` methods, and exposes them on **`/mcp`**.

- **ContactTools.cs**

  - Decorated with `[McpServerToolType]` to be discovered.
  - Two public methods marked with `[McpServerTool(Name = "...")]` to expose tools.
  - Uses a simple in-memory `List<Contact>` to store data.

---

## Troubleshooting

- **Claude can't see the tool / no tools appear**

  - Make sure the server is running on `http://localhost:5096/mcp`.
  - Restart Claude Desktop after editing the config.
  - Check that `npx` is available (`node -v` / `npm -v` work).
  - Validate the JSON config (no trailing commas, proper quotes).

- **Port conflict / server won't start**

  - Another process might be using port `5096`. Change `applicationUrl` in `launchSettings.json`, or run with:

    ```bash
    ASPNETCORE_URLS=http://localhost:6000 dotnet run --project ManageContacts.McpServer
    ```

    Then update the Claude config URL accordingly.

- **Tools reset after restart**

  - Expected: storage is in-memory for tutorial simplicity.

---

## Credits

- Built with **ModelContextProtocol** (`ModelContextProtocol` and `ModelContextProtocol.AspNetCore` – `0.3.0-preview.3`).
- Tested via **Claude Desktop** using **`mcp-remote`**.
