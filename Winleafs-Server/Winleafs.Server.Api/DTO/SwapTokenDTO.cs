namespace Winleafs.Server.Api.DTO
{
    public class SwapTokenDTO
    {
        public string grant_type { get; set; }

        public string code { get; set; }

        public string redirect_uri { get; set; }
    }
}
