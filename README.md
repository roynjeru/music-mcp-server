# music-mcp-server

A small Model Context Protocol (MCP) server that exposes Spotify-related tools for authenticated clients.

This project implements a server that registers MCP tools (implemented in `server/Tools/SpotifyTools.cs`) and exposes them over the MCP transport. The server expects callers to authenticate with a JWT-style bearer token and will forward that token to a specialized OAuth service to exchange or validate it before calling the Spotify API on the caller's behalf.

Key points
- Implements MCP server tooling (ModelContextProtocol.Server).
- Authn/Authz: accepts incoming bearer tokens (JWT) and integrates with ASP.NET Core JWT authentication.
- Token exchange: forwards the incoming bearer token to a specialized OAuth server (see link below) to exchange/validate for a Spotify access token, then calls Spotify using a per-request Authorization header.

Specialized OAuth server
- This project is designed to work with a companion OAuth server that you authored: https://github.com/roynjeru/oauth-music-streaming-mcp-server
- That OAuth server performs token exchange/validation and exposes an endpoint (used by this server) to exchange an incoming token for a Spotify access token.

Configuration
- Set the following configuration keys (for example in `appsettings.json` or environment variables):
	- `jwt-authority` — the authority/issuer URL for incoming JWTs (the OAuth server or identity provider).
	- `serverUrl` — a public URL for the MCP resource metadata (used when registering MCP resource metadata).

Quick start (macOS / zsh)

1. Restore and build

```bash
cd server
dotnet restore
dotnet build
```

2. Run the server (development)

```bash
cd server
dotnet run
```

3. How clients should call tools

- Clients must authenticate and include an Authorization header with a bearer token issued by your identity provider or the companion OAuth server.
- This server reads the incoming `Authorization: Bearer <token>` header, forwards the token to the configured OAuthServer exchange endpoint, and uses the returned Spotify access token when calling the Spotify API.

Example (high-level):

1) Obtain a bearer token from your OAuth server (see companion repo for details).
2) Call the MCP tool endpoint (the MCP transport exposes tool invocation endpoints). Include the same bearer token in the `Authorization` header. The server will perform the exchange and call Spotify for you.

More details
- See `server/Tools/SpotifyTools.cs` for the implementation of the Search tool and how the incoming Authorization header is read and used.
- See `Program.cs` for service registration and the HTTP client names used (`SpotifyApi`, `OAuthServer`).

Contributing
- Bug reports and PRs welcome. If you change the token exchange API in the OAuth server, update the corresponding request path in `SpotifyTools.cs`.

License
- MIT-style (no license file included by default).