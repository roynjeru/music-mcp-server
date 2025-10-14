namespace src 
{
    public static class Constants
    {
        public const string SearchToolDescription = "'q' string Required Your search query. You can narrow down your search using field filters. The available filters are album, artist, track, year, upc, tag:hipster, tag:new, isrc, and genre. Each field filter only applies to certain result types.The artist and year filters can be used while searching albums, artists and tracks. You can filter on a single year or a range (e.g. 1955-1960).The album filter can be used while searching albums and tracks.The genre filter can be used while searching artists and tracks.The isrc and track filters can be used while searching tracks.The upc, tag:new and tag:hipster filters can only be used while searching albums. The tag:new filter will return albums released in the past two weeks and tag:hipster can be used to return only albums with the lowest 10% popularity.Example: q=remaster%2520track%3ADoxy%2520artist%3AMiles%2520Davis 'type' array of strings Required A comma-separated list of item types to search across. Search results include hits from all the specified item types. For example: q=abacab&type=album,track returns both albums and tracks matching 'abacab'. Allowed values: 'album', 'artist', 'playlist', 'track', 'show', 'episode', 'audiobook'";
    }
}
