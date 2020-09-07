using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using W4k.AspNetCore.Correlator.Context.Types;
using W4k.AspNetCore.Correlator.Logging;
using W4k.AspNetCore.Correlator.Options;
using W4k.AspNetCore.Correlator.Validation;

namespace W4k.AspNetCore.Correlator.Context
{
    internal class CorrelationContextFactory : ICorrelationContextFactory
    {
        private readonly CorrelatorOptions _options;
        private readonly ICorrelationValidator? _validator;
        private readonly ILogger<CorrelationContextFactory> _logger;

        public CorrelationContextFactory(
            IOptions<CorrelatorOptions> options,
            ILogger<CorrelationContextFactory> logger)
            : this(options, null, logger)
        {
        }

        public CorrelationContextFactory(
            IOptions<CorrelatorOptions> options,
            ICorrelationValidator? validator,
            ILogger<CorrelationContextFactory> logger)
        {
            _options = options.Value;
            _validator = validator;
            _logger = logger;
        }

        public CorrelationContext CreateContext(HttpContext httpContext)
        {
            if (!TryGetCorrelationHeader(httpContext.Request.Headers, out var headerName, out var headerValue))
            {
                _logger.NoCorrelationHeaderReceived();
                return HandleEmptyValue(httpContext);
            }

            if (_validator != null)
            {
                var validationResult = _validator.Validate(headerValue);
                if (!validationResult.IsValid)
                {
                    _logger.InvalidCorrelationValue(headerName, validationResult.Reason);
                    return new InvalidCorrelationContext(headerName, validationResult);
                }
            }

            _logger.CorrelationIdReceived(headerName, headerValue!);
            return new RequestCorrelationContext(CorrelationId.FromString(headerValue), headerName);
        }

        private bool TryGetCorrelationHeader(
            IHeaderDictionary requestHeaders,
            [NotNullWhen(true)] out string? headerName,
            [NotNullWhen(true)] out string? headerValue)
        {
            if (requestHeaders.Count == 0)
            {
                headerName = headerValue = null;
                return false;
            }

            foreach (var header in _options.ReadFrom)
            {
                if (!requestHeaders.ContainsKey(header))
                {
                    continue;
                }

                var values = requestHeaders[header];
                if (values.Count > 0 && !string.IsNullOrEmpty(values[0]))
                {
                    headerName = header;
                    headerValue = values[0];

                    return true;
                }
            }

            headerName = headerValue = null;
            return false;
        }

        private CorrelationContext HandleEmptyValue(HttpContext httpContext)
        {
            var generateCorrelationId = _options.Factory;
            if (generateCorrelationId is null)
            {
                _logger.NoCorrelationIdFactoryConfigured();
                return EmptyCorrelationContext.Instance;
            }

            _logger.GeneratingCorrelationId();
            return new GeneratedCorrelationContext(generateCorrelationId(httpContext));
        }
    }
}
