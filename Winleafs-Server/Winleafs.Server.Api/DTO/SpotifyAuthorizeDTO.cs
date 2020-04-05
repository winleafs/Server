namespace Winleafs.Server.Api.DTO
{
    public class SpotifyAuthorizeDTO
    {
        public string scope { get; set; }

        public string state { get; set; }

        public bool show_dialog { get; set; }
    }
}
