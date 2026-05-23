using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using UserManagement.API.Data;

namespace UserManagement.API.Middlewares
{
    public class UserActivityMiddleware
    {
        private readonly RequestDelegate _next;

        public UserActivityMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            AppDbContext dbContext,
            IConfiguration configuration)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (int.TryParse(userIdClaim, out var userId))
                {
                    var user = await dbContext.Users
                        .FirstOrDefaultAsync(x => x.Id == userId);

                    if (user == null || user.IsDeleted || !user.Status)
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            message = "Account is inactive or does not exist."
                        });
                        return;
                    }

                    if (!user.LastActivityAt.HasValue)
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            message = "Session expired. Please login again."
                        });
                        return;
                    }

                    var idleTimeoutMinutes = configuration.GetValue<int>("Jwt:IdleTimeoutMinutes", 15);
                    var now = DateTime.UtcNow;
                    var idleTime = now - user.LastActivityAt.Value;

                    if (idleTime.TotalMinutes > idleTimeoutMinutes)
                    {
                        user.RefreshToken = null;
                        user.RefreshTokenExpiresAt = null;
                        user.LastActivityAt = null;

                        await dbContext.SaveChangesAsync();

                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            message = "Session expired due to inactivity."
                        });
                        return;
                    }

                    user.LastActivityAt = now;
                    await dbContext.SaveChangesAsync();
                }
            }

            await _next(context);
        }
    }
}