using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Winleafs.Server.Api.DTO;
using Winleafs.Server.Services.Exceptions;
using Winleafs.Server.Services.Helpers;
using Winleafs.Server.Services.Interfaces;

namespace Winleafs.Server.Api.Controllers
{
    [Route("api/[controller]")]
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
        [Route("authorize")]
        public async Task<IActionResult> Authorize([FromQuery]WinleafsIdDTO winleafsIdDTO)
        {
            //Add the user if it is his/her first time using the spotify functionality
            await _userService.AddUserIfNotExists(winleafsIdDTO.ApplicationId);

            const string scope = "playlist-read-private playlist-read-collaborative user-read-currently-playing user-read-playback-state";
            return Redirect($"https://accounts.spotify.com/authorize/?client_id={SpotifyClientInfo.ClientID}&response_type=code&redirect_uri={SpotifyClientInfo.RedirectURI}&scope={scope}&state={winleafsIdDTO.ApplicationId}&show_dialog=false");
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
        [Route("playlists")]
        public async Task<IActionResult> GetPlaylistNames([FromQuery]WinleafsIdDTO winleafsIdDTO)
        {
            try
            {
                return Ok(await _spotifyService.GetPlaylists(winleafsIdDTO.ApplicationId));
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
        [Route("current-playing-playlist-id")]
        public async Task<IActionResult> GetCurrentPlayingPlaylistId([FromQuery]WinleafsIdDTO winleafsIdDTO)
        {
            try
            {
                return Ok(await _spotifyService.GetCurrentPlayingPlaylistId(winleafsIdDTO.ApplicationId));
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

        [HttpPost]
        [Route("disconnect")]
        public async Task<IActionResult> Disconnect([FromBody]WinleafsIdDTO winleafsIdDTO)
        {
            await _spotifyService.Disconnect(winleafsIdDTO.ApplicationId);
            return Ok();
        }
    }
}
