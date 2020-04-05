namespace Winleafs.Server.Api.DTO
{
    public class RefreshTokenToSpotifyDTO
    {
        public string grant_type { get; set; }

        public string refresh_token { get; set; }
    }
}
