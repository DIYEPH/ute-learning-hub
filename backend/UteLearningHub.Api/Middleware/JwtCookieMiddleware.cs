namespace UteLearningHub.Api.Middleware;

public class JwtCookieMiddleware
{
    private readonly RequestDelegate _next;

    public JwtCookieMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Chỉ thêm token từ cookie nếu chưa có Authorization header
        // Điều này cho phép Scalar UI override bằng token được nhập vào
        if (!context.Request.Headers.ContainsKey("Authorization"))
        {
            // Thử đọc token từ các cookie names phổ biến
            var token = context.Request.Cookies["access_token"] 
                     ?? context.Request.Cookies["jwt_token"] 
                     ?? context.Request.Cookies["token"]
                     ?? context.Request.Cookies["auth_token"];

            if (!string.IsNullOrEmpty(token))
            {
                // Thêm Bearer prefix nếu chưa có
                var bearerToken = token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) 
                    ? token 
                    : $"Bearer {token}";
                
                context.Request.Headers.Add("Authorization", bearerToken);
            }
        }

        await _next(context);
    }
}

