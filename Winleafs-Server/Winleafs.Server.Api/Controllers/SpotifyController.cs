using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Winleafs.Server.Api.DTO;

namespace Winleafs.Server.Api.Controllers
{
    public class SpotifyController : BaseApiController
    {
        private readonly IConfiguration _configuration;

        public SpotifyController(IConfiguration config)
        {
            _configuration = config;
        }

        [HttpPost]
        [Route("swap")]
        public async Task<IActionResult> SwapToken([FromBody]string grant_type, [FromBody]string code, [FromBody]string redirect_uri)
        {
            using (var client = new HttpClient())
            {
                var swapTokenDTO = new SwapTokenDTO
                {
                    code = code,
                    grant_type = grant_type,
                    redirect_uri = redirect_uri
                };

                var queryString = new StringContent(JsonConvert.SerializeObject(swapTokenDTO), Encoding.UTF8, "application/json");

                var response = await client.PostAsync("https://accounts.spotify.com/api/token", queryString);
            }
        }

        [HttpPost]
        [Route("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody]string grant_type, [FromBody]string refresh_token)
        {

        }

        private string GetSpotifyAuthorizationHeader()
        {
            var spotifyauthorizationBytes = Encoding.UTF8.GetBytes($"{_configuration.GetValue<string>("Spotify:ClientId")}:{_configuration.GetValue<string>("Spotify:ClientSecret")}");
            return Convert.ToBase64String(spotifyauthorizationBytes);
        }
    }
}
