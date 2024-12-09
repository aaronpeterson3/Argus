using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace Argus.Api.Middleware
{
    public class ApiSecurityMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiSecurityMiddleware> _logger;
        private readonly ApiSecurityOptions _options;

        public ApiSecurityMiddleware(RequestDelegate next, ILogger<ApiSecurityMiddleware> logger, IOptions<ApiSecurityOptions> options)
        {
            _next = next;
            _logger = logger;
            _options = options.Value;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!await ValidateRequest(context))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            await _next(context);
        }

        private async Task<bool> ValidateRequest(HttpContext context)
        {
            // Content type validation
            if (context.Request.HasFormContentType)
            {
                if (!ValidateContentType(context.Request.ContentType))
                {
                    _logger.LogWarning("Invalid content type: {ContentType}", context.Request.ContentType);
                    return false;
                }
            }

            // Request size validation
            if (context.Request.ContentLength > _options.MaxRequestSize)
            {
                _logger.LogWarning("Request size exceeds limit: {Size}", context.Request.ContentLength);
                return false;
            }

            // Input validation
            if (!await ValidateInput(context.Request))
            {
                return false;
            }

            return true;
        }

        private bool ValidateContentType(string contentType)
        {
            return _options.AllowedContentTypes.Any(allowed =>
                contentType.StartsWith(allowed, StringComparison.OrdinalIgnoreCase));
        }

        private async Task<bool> ValidateInput(HttpRequest request)
        {
            if (request.HasFormContentType)
            {
                var form = await request.ReadFormAsync();
                foreach (var key in form.Keys)
                {
                    if (!ValidateString(form[key].ToString()))
                    {
                        _logger.LogWarning("Invalid input detected in form field: {Field}", key);
                        return false;
                    }
                }
            }

            if (request.Query.Count > 0)
            {
                foreach (var key in request.Query.Keys)
                {
                    if (!ValidateString(request.Query[key].ToString()))
                    {
                        _logger.LogWarning("Invalid input detected in query parameter: {Parameter}", key);
                        return false;
                    }
                }
            }

            return true;
        }

        private bool ValidateString(string input)
        {
            // Check for common injection patterns
            if (_options.BlockedPatterns.Any(pattern =>
                Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase)))
            {
                return false;
            }

            return true;
        }
    }

    public class ApiSecurityOptions
    {
        public long MaxRequestSize { get; set; } = 10 * 1024 * 1024; // 10MB
        public List<string> AllowedContentTypes { get; set; } = new()
        {
            "application/json",
            "multipart/form-data",
            "application/x-www-form-urlencoded"
        };
        public List<string> BlockedPatterns { get; set; } = new()
        {
            @"<script[^>]*>.*?</script>",
            @"javascript:",
            @"onload=",
            @"eval\(",
            @"(?:union|select|insert|update|delete|drop|alter).*?(?:from|into|table)"
        };
    }
}