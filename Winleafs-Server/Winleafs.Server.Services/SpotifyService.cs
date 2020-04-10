using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Winleafs.Server.Services.Interfaces;

namespace Winleafs.Server.Services
{
    public class SpotifyService : ISpotifyService
    {
        private IUserService _userService;

        public SpotifyService(IUserService userService)
        {
            _userService = userService;
        }

        public async Task SwapToken(string code, string state)
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
    }
}
