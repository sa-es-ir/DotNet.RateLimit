namespace DotNet.RateLimiter
{
    public enum RateLimitScope
    {
        /// <summary>
        /// rate limit will work for each action
        /// </summary>
        Action,

        /// <summary>
        /// rate limit will work for entire controller no matter which action calls
        /// </summary>
        Controller
    }
}