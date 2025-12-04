using DotNet.RateLimiter.Models;
using System.Text.Json;

namespace DotNet.RateLimiter.Utilities;

/// <summary>
/// Utility class for building rate limit response with custom structure
/// </summary>
internal static class RateLimitResponseBuilder
{
    /// <summary>
    /// Builds the response string based on options configuration
    /// </summary>
    /// <param name="options">Rate limit options containing response configuration</param>
    /// <returns>JSON string of the response</returns>
    public static string BuildResponse(RateLimitOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ResponseStructure))
        {
            // Use default response structure for backward compatibility
            var defaultResponse = new RateLimitResponse
            {
                Code = options.HttpStatusCode,
                Message = options.ErrorMessage
            };
            return JsonSerializer.Serialize(defaultResponse);
        }

        // Replace placeholders with actual values
        var response = options.ResponseStructure
            .Replace("$(ErrorMessage)", options.ErrorMessage)
            .Replace("$(HttpStatusCode)", options.HttpStatusCode.ToString());

        return response;
    }
}