namespace DotNet.RateLimiter
{
    public enum RateLimitScope
    {
        /// <summary>
        /// Rate limit will work on each action
        /// </summary>
        Action,

        /// <summary>
        /// Rate limit will work on the entire controller no matter which action calls
        /// </summary>
        Controller
    }
}