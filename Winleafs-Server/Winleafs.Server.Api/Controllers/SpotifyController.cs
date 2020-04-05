using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Winleafs.Server.Api.Configuration;
using Winleafs.Server.Api.DTO;

namespace Winleafs.Server.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpotifyController : BaseApiController
    {
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Authorize([FromQuery]SpotifyAuthorizeDTO spotifyAuthorizeDTO)
        {
            return Redirect($"https://accounts.spotify.com/authorize/?client_id={SpotifyClientInfo.ClientID}&response_type=code&redirect_uri={SpotifyClientInfo.RedirectURI}&scope={spotifyAuthorizeDTO.scope}&state={spotifyAuthorizeDTO.state}&show_dialog={spotifyAuthorizeDTO.show_dialog}");
        }

        [HttpGet]
        [Route("swap")]
        public async Task<IActionResult> SwapToken([FromQuery]string code)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", GetSpotifyAuthorizationHeader());

                var postDictionary = new Dictionary<string, string>
                {
                    { "code", code },
                    { "grant_type", "authorization_code" },
                    { "redirect_uri", SpotifyClientInfo.RedirectURI }
                };

                var response = await client.PostAsync("https://accounts.spotify.com/api/token", new FormUrlEncodedContent(postDictionary));

                var result = JsonConvert.DeserializeObject<SwapTokenResultDTO>(await response.Content.ReadAsStringAsync());

                return Ok(result);
            }
        }

        [HttpGet]
        [Route("refresh")]
        public async Task<IActionResult> RefreshToken([FromQuery]string refresh_token)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", GetSpotifyAuthorizationHeader());

                var postDictionary = new Dictionary<string, string>
                {
                    { "refresh_token", refresh_token },
                    { "grant_type", "refresh_token" },
                };

                var response = await client.PostAsync("https://accounts.spotify.com/api/token", new FormUrlEncodedContent(postDictionary));

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
