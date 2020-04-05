﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    }
}