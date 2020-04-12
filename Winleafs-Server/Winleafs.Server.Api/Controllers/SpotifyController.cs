using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Winleafs.Server.Services.Exceptions;
using Winleafs.Server.Services.Helpers;
using Winleafs.Server.Services.Interfaces;

namespace Winleafs.Server.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SpotifyController : BaseApiController
    {
        private IUserService _userService;
        private ISpotifyService _spotifyService;

        public SpotifyController(DbContext context, IUserService userService, ISpotifyService spotifyService) : base(context)
        {
            _userService = userService;
            _spotifyService = spotifyService;
        }

        /// <remarks>
        /// Must be GET due to Spotify authorization standards
        /// </remarks>
        [HttpGet]
        [Route("authorize/{applicationId}")]
        public async Task<IActionResult> Authorize(string applicationId)
        {
            //Add the user if it is his/her first time using the spotify functionality
            await _userService.AddUserIfNotExists(applicationId);

            const string scope = "playlist-read-private playlist-read-collaborative user-read-currently-playing user-read-playback-state";
            return Redirect($"https://accounts.spotify.com/authorize/?client_id={SpotifyClientInfo.ClientID}&response_type=code&redirect_uri={SpotifyClientInfo.RedirectURI}&scope={scope}&state={applicationId}&show_dialog=false");
        }

        /// <remarks>
        /// Must be GET due to Spotify authorization standards
        /// </remarks>
        [HttpGet]
        [Route("swap")]
        public async Task<IActionResult> SwapToken([FromQuery]string code, [FromQuery]string state, [FromQuery]string error)
        {
            //If there is an error, the user did not grant access
            if (!string.IsNullOrEmpty(error))
            {
                return BadRequest("Error: Winleafs did not get access to Spotify.");
            }

            try
            {
                await _spotifyService.SwapToken(code, state); //We passed the application id as state for the authorize request
            }
            catch (InvalidApplicationIdException e)
            {
                //TODO: add logging
                return BadRequest("Failed to find a matching application id. Please disconnect from Spotify and try again.");
            }
            catch
            {
                //TODO: add logging
                return BadRequest("Unknown error during Spotify authorization. Please disconnect from Spotify and try again.");
            }

            return Ok("Authentication successful! You can now close this window");
        }

        [HttpGet]
        [Route("playlists/{applicationId}")]
        public async Task<IActionResult> GetPlaylistNames(string applicationId)
        {
            try
            {
                return Ok(await _spotifyService.GetPlaylists(applicationId));
            }
            catch (InvalidApplicationIdException e)
            {
                //TODO: add logging
                return BadRequest("Failed to find a matching application id. Please disconnect from Spotify and try again.");
            }
            catch
            {
                //TODO: add logging
                return BadRequest("Failed to retrieve playlist names.");
            }
        }

        [HttpGet]
        [Route("current-playing-playlist-id/{applicationId}")]
        public async Task<IActionResult> GetCurrentPlayingPlaylistId(string applicationId)
        {
            try
            {
                return Ok(await _spotifyService.GetCurrentPlayingPlaylistId(applicationId));
            }
            catch (InvalidApplicationIdException e)
            {
                //TODO: add logging
                return BadRequest("Failed to find a matching application id. Please disconnect from Spotify and try again.");
            }
            catch
            {
                //TODO: add logging
                return BadRequest("Failed to retrieve current playing playlist id.");
            }
        }

        [HttpDelete]
        [Route("disconnect/{applicationId}")]
        public async Task<IActionResult> Disconnect(string applicationId)
        {
            await _spotifyService.Disconnect(applicationId);
            return Ok();
        }

        [HttpGet]
        [Route("is-connected/{applicationId}")]
        public async Task<IActionResult> IsConnected(string applicationId)
        {
            return Ok((await _spotifyService.IsConnected(applicationId)).ToString().ToLower());
        }
    }
}
