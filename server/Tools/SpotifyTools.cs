using System.ComponentModel;
using ModelContextProtocol.Server;

namespace server.Tools
{
    [McpServerToolType]
    public sealed class SpotifyTools
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public SpotifyTools(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        
        [McpServerTool, Description("Get response from tool")]
        public async Task<string> CallTool()
        {
            // get the client 
            var client = _httpClientFactory.CreateClient("SpotifyApi");

            return "completed";
        }
    }
}