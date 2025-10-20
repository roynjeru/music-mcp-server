using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text.Json;
using ModelContextProtocol.Server;
using server.Models;
using src;

namespace server.Tools
{
    [McpServerToolType]
    public sealed class SpotifyTools
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly ILogger<SpotifyTools> _logger;

        public SpotifyTools(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            ILogger<SpotifyTools> logger)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        [McpServerTool]
        [Description("Get Spotify catalog information about albums, artists, playlists, tracks, shows, episodes or audiobooks that match a keyword string. The request body should be a URL-encoded query string as described in the Spotify Search API documentation (e.g., 'q=beatles&type=artist').")]
        public async Task<string> Search([Description(Constants.SearchToolDescription)] string queryString)
        {
            _logger.LogInformation("Query string from mcpclient {queryString}", queryString);
            // get the clients
            var spotifyClient = _httpClientFactory.CreateClient("SpotifyApi");
            var oauthClient = _httpClientFactory.CreateClient("OAuthServer");

            // read incoming Authorization header from the current HTTP context
            var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();
            _logger.LogInformation("Authorization Header: {AuthHeader}", authHeader);

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("No valid Authorization header found. {authHeader}", authHeader);
                throw new UnauthorizedAccessException("No valid Authorization header found.");
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();

            oauthClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Exchange token if needed (as in your original code)
            var exchangeTokenRequest = new HttpRequestMessage(HttpMethod.Post, "/exchange/spotify-token");

            var response = await oauthClient.SendAsync(exchangeTokenRequest).ConfigureAwait(false);
            var exchangeTokenResponse = await response.Content.ReadFromJsonAsync<ExchangeTokenResponse>();

            // Use the exchanged token to call Spotify Search API
            var searchRequest = new HttpRequestMessage(HttpMethod.Get, $"/v1/search?{queryString}");
            searchRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", exchangeTokenResponse?.AccessToken);

            var searchResponse = await spotifyClient.SendAsync(searchRequest);
            searchResponse.EnsureSuccessStatusCode();

            var result = await searchResponse.Content.ReadAsStringAsync();
            return result;
        }

        [McpServerTool]
        [Description("Get a list of the playlists owned or followed by the current Spotify user.")]
        public async Task<string> GetCurrentUsersPlaylists()
        {
            // get the clients
            var spotifyClient = _httpClientFactory.CreateClient("SpotifyApi");
            var oauthClient = _httpClientFactory.CreateClient("OAuthServer");

            // read incoming Authorization header from the current HTTP context
            var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();
            _logger.LogInformation("Authorization Header: {AuthHeader}", authHeader);

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("No valid Authorization header found. {authHeader}", authHeader);
                throw new UnauthorizedAccessException("No valid Authorization header found.");
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();

            oauthClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Exchange token if needed (as in your original code)
            var exchangeTokenRequest = new HttpRequestMessage(HttpMethod.Post, "/exchange/spotify-token");

            var response = await oauthClient.SendAsync(exchangeTokenRequest).ConfigureAwait(false);
            var exchangeTokenResponse = await response.Content.ReadFromJsonAsync<ExchangeTokenResponse>();

            // Use the exchanged token to call Spotify API
            var mePlaylistsRequest = new HttpRequestMessage(HttpMethod.Get, "/me/playlists");
            mePlaylistsRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", exchangeTokenResponse?.AccessToken);

            var mePlaylistsResponse = await spotifyClient.SendAsync(mePlaylistsRequest);
            mePlaylistsResponse.EnsureSuccessStatusCode();

            var result = await mePlaylistsResponse.Content.ReadAsStringAsync();
            return result;
        }

        [McpServerTool]
        [Description("Add one or more items to the user's playlist")]
        public async Task<string> AddItemsToPlaylist([Description(Constants.AddItemsToPlaylistDescription)] string playlistId, [Description(Constants.UrisDescription)] string[] uris)
        {
            _logger.LogInformation("Query string from mcpclient {playlistId}", playlistId);
            // get the clients
            var spotifyClient = _httpClientFactory.CreateClient("SpotifyApi");
            var oauthClient = _httpClientFactory.CreateClient("OAuthServer");

            // read incoming Authorization header from the current HTTP context
            var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();
            _logger.LogInformation("Authorization Header: {AuthHeader}", authHeader);

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("No valid Authorization header found. {authHeader}", authHeader);
                throw new UnauthorizedAccessException("No valid Authorization header found.");
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();

            oauthClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Exchange token if needed (as in your original code)
            var exchangeTokenRequest = new HttpRequestMessage(HttpMethod.Post, "/exchange/spotify-token");

            var response = await oauthClient.SendAsync(exchangeTokenRequest).ConfigureAwait(false);
            var exchangeTokenResponse = await response.Content.ReadFromJsonAsync<ExchangeTokenResponse>();

            // Use the exchanged token to call Spotify API
            var addItemsRequest = new HttpRequestMessage(HttpMethod.Post, $"/playlists/{playlistId}/tracks");
            StringContent content = new(JsonSerializer.Serialize(uris));
            _logger.LogInformation("String content looks like: {content}", content);
            addItemsRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", exchangeTokenResponse?.AccessToken);
            addItemsRequest.Content = content;

            var addItemsResponse = await spotifyClient.SendAsync(addItemsRequest);
            addItemsResponse.EnsureSuccessStatusCode();

            var result = await addItemsResponse.Content.ReadAsStringAsync();
            return result;
        }
    }
}