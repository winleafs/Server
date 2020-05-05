using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
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
            Guid guidOutput;
            if (string.IsNullOrWhiteSpace(applicationId) || !Guid.TryParse(applicationId, out guidOutput))
            {
                return BadRequest("Error: Invalid parameters given."); //Generic error to not give too much information
            }

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

            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(state))
            {
                return BadRequest("Error: Invalid parameters given."); //Generic error to not give too much information
            }

            try
            {
                await _spotifyService.SwapToken(code, state); //We passed the application id as state for the authorize request
            }
            catch (InvalidApplicationIdException ex)
            {
                Log.Warning(ex, $"Failed to find a matching application id for application id {state}.");
                return BadRequest("Failed to find a matching application id. Please disconnect from Spotify and try again.");
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Unknown error during Spotify authorization.");
                return BadRequest("Unknown error during Spotify authorization. Please disconnect from Spotify and try again.");
            }

            return Ok("Authentication successful! You can now close this window");
        }

        [HttpGet]
        [Route("playlists/{applicationId}")]
        public async Task<IActionResult> GetPlaylistNames(string applicationId)
        {
            Guid guidOutput;
            if (string.IsNullOrWhiteSpace(applicationId) || !Guid.TryParse(applicationId, out guidOutput))
            {
                return BadRequest("Error: Invalid parameters given."); //Generic error to not give too much information
            }

            try
            {
                return Ok(await _spotifyService.GetPlaylists(applicationId));
            }
            catch (InvalidApplicationIdException ex)
            {
                Log.Warning(ex, $"Failed to find a matching application id for application id {applicationId}.");
                return BadRequest("Failed to find a matching application id. Please disconnect from Spotify and try again.");
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to retrieve playlist names.");
                return BadRequest("Failed to retrieve playlist names.");
            }
        }

        [HttpGet]
        [Route("current-playing-playlist-id/{applicationId}")]
        public async Task<IActionResult> GetCurrentPlayingPlaylistId(string applicationId)
        {
            Guid guidOutput;
            if (string.IsNullOrWhiteSpace(applicationId) || !Guid.TryParse(applicationId, out guidOutput))
            {
                return BadRequest("Error: Invalid parameters given."); //Generic error to not give too much information
            }

            try
            {
                return Ok(await _spotifyService.GetCurrentPlayingPlaylistId(applicationId));
            }
            catch (InvalidApplicationIdException ex)
            {
                Log.Warning(ex, $"Failed to find a matching application id for application id {applicationId}.");
                return BadRequest("Failed to find a matching application id. Please disconnect from Spotify and try again.");
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to retrieve current playing playlist id.");
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
