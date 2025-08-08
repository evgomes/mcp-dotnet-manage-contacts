var builder = WebApplication.CreateBuilder(args);

// Configure services for the MCP server.
builder
    .Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

// Maps the "/mcp" endpoint to respond MCP calls.
app.MapMcp("/mcp");

app.Run();