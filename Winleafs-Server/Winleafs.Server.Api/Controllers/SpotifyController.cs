using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Winleafs.Server.Api.Configuration;
using Winleafs.Server.Api.DTO;
using Winleafs.Server.Services;

namespace Winleafs.Server.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpotifyController : BaseApiController
    {
        private IUserService _userService;

        public SpotifyController(DbContext context, IUserService userService) : base(context)
        {
            _userService = userService;
        }

        [HttpGet]
        [Route("authorize")]
        public async Task<IActionResult> Authorize([FromQuery]WinleafsIdDTO winleafsIdDTO)
        {
            //Add the user if it is his/her first time using the spotify functionality
            await _userService.AddUserIfNotExists(winleafsIdDTO.ApplicationId);

            const string scope = "playlist-read-private playlist-read-collaborative user-read-currently-playing user-read-playback-state";
            return Redirect($"https://accounts.spotify.com/authorize/?client_id={SpotifyClientInfo.ClientID}&response_type=code&redirect_uri={SpotifyClientInfo.RedirectURI}&scope={scope}&state={winleafsIdDTO.ApplicationId}&show_dialog=false");
        }

        [HttpGet]
        [Route("swap")]
        public async Task<IActionResult> SwapToken([FromQuery]string code, [FromQuery]string state, [FromQuery]string error)
        {
            //TODO: error handling if error != empty

            var user = await _userService.FindUserByApplicationId(state); //We passed the application id as state for the authorize request

            if (user == null)
            {
                return BadRequest("Invalid state");
            }

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

                user.RefreshToken = result.refresh_token;
                user.SpotifyAccessToken = result.access_token;
                user.SpotifyExpiresOn = DateTime.Now.AddSeconds(result.expires_in);

                await Context.SaveChangesAsync();

                return Ok("Authentication succesfull! You can now close this window");
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

                //return Ok(result);
                //TODO redirect to localhost:4200/auth
            }
        }

        private string GetSpotifyAuthorizationHeader()
        {
            var spotifyauthorizationBytes = Encoding.UTF8.GetBytes($"{SpotifyClientInfo.ClientID}:{SpotifyClientInfo.ClientSecret}");
            return Convert.ToBase64String(spotifyauthorizationBytes);
        }
    }
}
