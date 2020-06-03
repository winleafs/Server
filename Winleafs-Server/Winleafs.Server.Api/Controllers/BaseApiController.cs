using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Net;
using Winleafs.Server.Api.DTO;
using Winleafs.Server.Data;

namespace Winleafs.Server.Api.Controllers
{
    public abstract class BaseApiController : ControllerBase
    {
        protected ApplicationContext Context;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseApiController" /> class.
        /// </summary>
        protected BaseApiController()
        {

        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseApiController" /> class.
        /// </summary>
        /// <param name="context">The injected context to be used.</param>
        protected BaseApiController(DbContext context)
        {
            if (context is ApplicationContext appContext) Context = appContext;
        }

        /// <summary>
        ///     Writes a warning log and returns a formatted bad request response
        /// </summary>
        protected IActionResult WarningLogBadRequest(string error, Exception ex = null)
        {
            if (ex != null)
            {
                Log.Warning(ex, error);
            }
            else
            {
                Log.Warning(error);
            }

            return BadRequest(new ErrorDTO
            {
                Message = error,
                ErrorCode = (int)HttpStatusCode.BadRequest
            });
        }
    }
}