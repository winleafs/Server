using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Winleafs.Server.Api.Configuration;
using Winleafs.Server.Api.DTO;

namespace Winleafs.Server.Api.Controllers
{
    public class SpotifyController : BaseApiController
    {
        [HttpPost]
        [Route("swap")]
        public async Task<IActionResult> SwapToken([FromBody]string grant_type, [FromBody]string code, [FromBody]string redirect_uri)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Basic " + GetSpotifyAuthorizationHeader());

                var swapTokenDTO = new SwapTokenToSpotifyDTO
                {
                    code = code,
                    grant_type = grant_type,
                    redirect_uri = redirect_uri
                };

                var queryString = new StringContent(JsonConvert.SerializeObject(swapTokenDTO), Encoding.UTF8, "application/json");

                var response = await client.PostAsync("https://accounts.spotify.com/api/token", queryString);

                var result = JsonConvert.DeserializeObject<SwapTokenResultDTO>(await response.Content.ReadAsStringAsync());

                return Ok(result);
            }
        }

        [HttpPost]
        [Route("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody]string grant_type, [FromBody]string refresh_token)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Basic " + GetSpotifyAuthorizationHeader());

                var swapTokenDTO = new RefreshTokenToSpotifyDTO
                {
                    grant_type = grant_type,
                    refresh_token = refresh_token
                };

                var queryString = new StringContent(JsonConvert.SerializeObject(swapTokenDTO), Encoding.UTF8, "application/json");

                var response = await client.PostAsync("https://accounts.spotify.com/api/token", queryString);

                var result = JsonConvert.DeserializeObject<RefreshTokenResultDTO>(await response.Content.ReadAsStringAsync());

                return Ok(result);
            }
        }

        private string GetSpotifyAuthorizationHeader()
        {
            var spotifyauthorizationBytes = Encoding.UTF8.GetBytes($"{SpotifyClientInfo.ClientID}:{SpotifyClientInfo.ClientSecret}");
            return Convert.ToBase64String(spotifyauthorizationBytes);
        }
    }
}
