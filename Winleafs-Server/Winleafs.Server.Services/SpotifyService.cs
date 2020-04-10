using Newtonsoft.Json;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Winleafs.Server.Data;
using Winleafs.Server.Models.Models;
using Winleafs.Server.Services.DTO;
using Winleafs.Server.Services.Exceptions;
using Winleafs.Server.Services.Helpers;
using Winleafs.Server.Services.Interfaces;

namespace Winleafs.Server.Services
{
    public class SpotifyService : ISpotifyService
    {
        private static HttpClient _spotifyAuthorizationClient = new HttpClient();

        private IUserService _userService;
        private ApplicationContext _applicationContext;

        public SpotifyService(IUserService userService, ApplicationContext applicationContext)
        {
            _userService = userService;
            _applicationContext = applicationContext;
        }

        public async Task SwapToken(string code, string applicationId)
        {
            var user = await _userService.FindUserByApplicationId(applicationId);

            if (user == null)
            {
                throw new InvalidApplicationIdException("No user was found for the given application id");
            }

            _spotifyAuthorizationClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", GetSpotifyAuthorizationHeader());

            var postDictionary = new Dictionary<string, string>
            {
                { "code", code },
                { "grant_type", "authorization_code" },
                { "redirect_uri", SpotifyClientInfo.RedirectURI }
            };

            var response = await _spotifyAuthorizationClient.PostAsync("https://accounts.spotify.com/api/token", new FormUrlEncodedContent(postDictionary));

            var result = JsonConvert.DeserializeObject<SwapTokenResultDTO>(await response.Content.ReadAsStringAsync());

            user.SpotifyRefreshToken = result.refresh_token;
            user.SpotifyAccessToken = result.access_token;
            user.SpotifyExpiresOn = DateTime.Now.AddSeconds(result.expires_in - 10); //Do minus 10 such that our value is always lower than the expire value on Spotify's side

            _userService.Update(user);

            await _applicationContext.SaveChangesAsync();
        }

        public async Task RefreshToken(User user)
        {
            _spotifyAuthorizationClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", GetSpotifyAuthorizationHeader());

            var postDictionary = new Dictionary<string, string>
            {
                { "refresh_token", user.SpotifyRefreshToken },
                { "grant_type", "refresh_token" },
            };

            var response = await _spotifyAuthorizationClient.PostAsync("https://accounts.spotify.com/api/token", new FormUrlEncodedContent(postDictionary));

            var result = JsonConvert.DeserializeObject<RefreshTokenResultDTO>(await response.Content.ReadAsStringAsync());

            user.SpotifyAccessToken = result.access_token;
            user.SpotifyExpiresOn = DateTime.Now.AddSeconds(result.expires_in - 10); //Do minus 10 such that our value is always lower than the expire value on Spotify's side

            _userService.Update(user);

            await _applicationContext.SaveChangesAsync();
        }

        public async Task<string> GetCurrentPlayingPlaylistId(string applicationId)
        {
            var webAPI = await GetRefreshedSpotifyWebAPI(applicationId);

            var playbackContext = await webAPI.GetPlaybackAsync();

            if (!playbackContext.IsPlaying)
            {
                return null;
            }

            var playlistUrl = playbackContext.Context.ExternalUrls.FirstOrDefault(externalUrl => externalUrl.Value.Contains("playlist"));
            return playlistUrl.Value.Remove(0, "https://open.spotify.com/playlist/".Length);
        }

        public async Task<IEnumerable<string>> GetPlaylistNames(string applicationId)
        {
            var webAPI = await GetRefreshedSpotifyWebAPI(applicationId);
            var profile = await webAPI.GetPrivateProfileAsync();

            var info = await webAPI.GetUserPlaylistsAsync(profile.Id);

            return info.Items.Select(playlist => playlist.Name);
        }

        #region Privates
        private string GetSpotifyAuthorizationHeader()
        {
            var spotifyauthorizationBytes = Encoding.UTF8.GetBytes($"{SpotifyClientInfo.ClientID}:{SpotifyClientInfo.ClientSecret}");
            return Convert.ToBase64String(spotifyauthorizationBytes);
        }

        /// <summary>
        /// Creates a <see cref="SpotifyWebAPI"/> object for the given
        /// <paramref name="applicationId"/> to query the Spotify API.
        /// Refreshes the user's access token if it expired.
        /// </summary>
        private async Task<SpotifyWebAPI> GetRefreshedSpotifyWebAPI(string applicationId)
        {
            var user = await _userService.FindUserByApplicationId(applicationId);

            if (user == null)
            {
                throw new InvalidApplicationIdException("No user was found for the given application id");
            }

            if (DateTime.Now > user.SpotifyExpiresOn)
            {
                await RefreshToken(user);
            }

            return new SpotifyWebAPI
            {
                TokenType = "Bearer", //TokenType is always bearar, hence we do not need to save it on a user object
                AccessToken = user.SpotifyAccessToken
            };
        }
        #endregion
    }
}
