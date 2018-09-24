﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using W4k.AspNetCore.Correlator.Extensions;
using W4k.AspNetCore.Correlator.Options;

namespace W4k.AspNetCore.Correlator
{
    /// <summary>
    /// Correlator middleware for reading correlation ID from incoming HTTP request.
    /// </summary>
    public class CorrelatorMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly CorrelatorOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelatorMiddleware"/> class.
        /// </summary>
        /// <param name="next">Delegate representing the next middleware in the request pipeline.</param>
        /// <param name="options">Correlator options.</param>
        public CorrelatorMiddleware(RequestDelegate next, IOptions<CorrelatorOptions> options)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Executes the middleware.
        /// </summary>
        /// <param name="httpContext">The <see cref="HttpContext"/> for the current request.</param>
        /// <returns>
        /// A task that represents the execution of this middleware.
        /// </returns>
        public async Task Invoke(HttpContext httpContext)
        {
            // TODO: Review extension methods (refactor?)

            // keep name of header from which correlation ID has been read
            (CorrelationId correlationId, string headerName) =
                httpContext.Request.ReadCorrelationId(_options.ReadFrom);

            // we may end up using correlation ID generated by ASP.NET
            correlationId = correlationId
                .GenerateIfEmpty(_options.Factory)
                .ApplyTo(httpContext);

            await _next.Invoke(httpContext);

            // emit correlation ID if not set by another middleware in pipeline (and if configured to do so)
            httpContext.Response.TryEmitCorrelationId(_options.Emit, headerName, correlationId);
        }
    }
}
