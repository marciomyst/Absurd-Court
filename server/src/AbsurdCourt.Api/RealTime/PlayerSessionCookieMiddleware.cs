using System.Security.Cryptography;

namespace AbsurdCourt.Api.RealTime;

public sealed class PlayerSessionCookieMiddleware(RequestDelegate next)
{
    public const string CookieName = "absurd-court-session";

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/hubs/court") &&
            !context.Request.Cookies.ContainsKey(CookieName))
        {
            var sessionId = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            context.Response.Cookies.Append(CookieName, sessionId, new CookieOptions
            {
                HttpOnly = true,
                Secure = context.Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                MaxAge = TimeSpan.FromDays(30),
                Path = "/",
            });
        }

        await next(context);
    }
}
