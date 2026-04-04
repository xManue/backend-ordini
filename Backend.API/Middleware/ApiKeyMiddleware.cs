namespace Backend.API.Middleware
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _apiKey;

        public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _apiKey = configuration["ApiKey"] ?? "1234";
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? "";

            // Public endpoints: client menu (GET products) and order creation/payment
            if (IsPublicEndpoint(context.Request.Method, path))
            {
                await _next(context);
                return;
            }

            // All other API endpoints require the API key
            if (path.StartsWith("/api", StringComparison.OrdinalIgnoreCase))
            {
                if (!context.Request.Headers.TryGetValue("X-Api-Key", out var providedKey) ||
                    providedKey.ToString() != _apiKey)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Chiave API non valida");
                    return;
                }
            }

            await _next(context);
        }

        private static bool IsPublicEndpoint(string method, string path)
        {
            var lower = path.ToLowerInvariant();

            // Client can read the product list
            if (method == "GET" && lower.Contains("/api/products/getproducts"))
                return true;

            // Client can read products by category
            if (method == "GET" && lower.Contains("/api/products/getproductsbycategory"))
                return true;

            // Client can create an order
            if (method == "POST" && lower.Contains("/api/orders/createorder"))
                return true;

            // Client can pay for their order
            if (method == "PATCH" && lower.Contains("/pay"))
                return true;

            // Static files and non-API paths are always public
            if (!lower.StartsWith("/api"))
                return true;

            return false;
        }
    }
}
