using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Winleafs.Server.Models.Models;
using Winleafs.Server.Services.DTO;
using Winleafs.Server.Services.Exceptions;
using Winleafs.Server.Services.Helpers;
using Winleafs.Server.Services.Interfaces;

namespace Winleafs.Server.Services
{
    public class SpotifyService : ISpotifyService
    {
        private const string _spotifyApiTokenUrl = "https://accounts.spotify.com/api/token";

        private readonly HttpClient _spotifyAuthorizationClient;

        private readonly IUserService _userService;
        private readonly DbContext _context;

        public SpotifyService(IUserService userService, DbContext context)
        {
            _userService = userService;
            _context = context;

            _spotifyAuthorizationClient = new HttpClient();
            _spotifyAuthorizationClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", GetSpotifyAuthorizationHeader());
        }

        public async Task SwapToken(string code, string applicationId)
        {
            var user = await _userService.FindUserByApplicationId(applicationId);

            if (user == null)
            {
                throw new InvalidApplicationIdException("No user was found for the given application id");
            }

            var postDictionary = new Dictionary<string, string>
            {
                { "code", code },
                { "grant_type", "authorization_code" },
                { "redirect_uri", SpotifyClientInfo.RedirectURI }
            };

            var response = await _spotifyAuthorizationClient.PostAsync(_spotifyApiTokenUrl, new FormUrlEncodedContent(postDictionary));

            CheckResponse(response);

            var result = JsonConvert.DeserializeObject<SwapTokenResultDTO>(await response.Content.ReadAsStringAsync());

            user.SpotifyRefreshToken = result.RefreshToken;
            user.SpotifyAccessToken = result.AccessToken;
            user.SpotifyExpiresOn = DateTime.Now.AddSeconds(result.ExpiresInSeconds - 10); //Do minus 10 such that our value is always lower than the expire value on Spotify's side

            _userService.Update(user);

            await _context.SaveChangesAsync();
        }

        public async Task RefreshToken(User user)
        {
            var postDictionary = new Dictionary<string, string>
            {
                { "refresh_token", user.SpotifyRefreshToken },
                { "grant_type", "refresh_token" },
            };

            var response = await _spotifyAuthorizationClient.PostAsync(_spotifyApiTokenUrl, new FormUrlEncodedContent(postDictionary));

            var result = JsonConvert.DeserializeObject<RefreshTokenResultDTO>(await response.Content.ReadAsStringAsync());

            CheckResponse(response);

            user.SpotifyAccessToken = result.AccessToken;
            user.SpotifyExpiresOn = DateTime.Now.AddSeconds(result.ExpiresInSeconds - 10); //Do minus 10 such that our value is always lower than the expire value on Spotify's side

            _userService.Update(user);

            await _context.SaveChangesAsync();
        }

        public async Task<string> GetCurrentPlayingPlaylistId(string applicationId)
        {
            var webAPI = await GetRefreshedSpotifyWebAPI(applicationId);
            var playbackContext = await webAPI.GetPlaybackAsync();

            CheckResponse(playbackContext);

            if (!playbackContext.IsPlaying)
            {
                return null;
            }

            var playlistUrl = playbackContext.Context.ExternalUrls.FirstOrDefault(externalUrl => externalUrl.Value.Contains("playlist"));
            return string.IsNullOrEmpty(playlistUrl.Value) ? null : playlistUrl.Value.Remove(0, "https://open.spotify.com/playlist/".Length);
        }

        public async Task<Dictionary<string, string>> GetPlaylists(string applicationId)
        {
            var webAPI = await GetRefreshedSpotifyWebAPI(applicationId);
            var profile = await webAPI.GetPrivateProfileAsync();

            CheckResponse(profile);

            var info = await webAPI.GetUserPlaylistsAsync(profile.Id);

            return info.Items.ToDictionary(playlist => playlist.Id, playlist => playlist.Name);
        }

        public async Task Disconnect(string applicationId)
        {
            var user = await _userService.FindUserByApplicationId(applicationId);

            if (user != null)
            {
                user.SpotifyAccessToken = null;
                user.SpotifyExpiresOn = null;
                user.SpotifyRefreshToken = null;

                _userService.Update(user);

                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsConnected(string applicationId)
        {
            try
            {
                var webAPI = await GetRefreshedSpotifyWebAPI(applicationId);
                var playbackContext = await webAPI.GetPlaybackAsync();

                CheckResponse(playbackContext);

                return playbackContext.Error == null;
            }
            catch
            {
                return false;
            }
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

            if (user.SpotifyAccessToken == null)
            {
                throw new SpotifyNotConnectedException("The user is not connected to Spoify; no Spotify information is saved for the user.");
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

        /// <summary>
        /// Throws and logs an exception if the statuscode is not 200 OK
        /// </summary>
        private void CheckResponse(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                //TODO: add logging with statuscode and reasonphrase
                throw new SpotifyRequestException("Something went wrong during a Spotify API request.");
            }
        }

        /// <summary>
        /// Throws and logs an exception if the request made with
        /// the Spotify Web API library is not successful.
        /// </summary>
        private void CheckResponse(BasicModel webAPIResponse)
        {
            if (webAPIResponse.HasError())
            {
                //TODO: add logging with statuscode and error
                throw new SpotifyRequestException("Something went wrong during a Spotify API request.");
            }
        }
        #endregion
    }
}
