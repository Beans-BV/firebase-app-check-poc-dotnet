using AppCheck.Helper.Attributes;
using AppCheck.Settings.Model.ResponseModel;
using FirebaseAdmin;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;

namespace AppCheck.Middleware
{
    public class FirebaseAppCheckMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<FirebaseAppCheckMiddleware> _logger;
        private const string _firebaseAppCheckJwksUrl = "https://firebaseappcheck.googleapis.com/v1/jwks";

        public FirebaseAppCheckMiddleware(RequestDelegate next, ILogger<FirebaseAppCheckMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.GetEndpoint()?.Metadata.GetMetadata<FirebaseAppCheckAttribute>() == null)
            {
                _logger.LogInformation("Regular method called without header");
                await _next(context);
                return;
            }

            string appCheckToken = context.Request.Headers["X-Firebase-AppCheck"].FirstOrDefault();
            _logger.LogInformation($"Header Token: {appCheckToken}");
            if (string.IsNullOrEmpty(appCheckToken) || !await IsValidAppCheckToken(appCheckToken))
            {
                _logger.LogInformation("Token not valid");
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                var responseModel = new ApiResponse() { Success = false, Message = "Invalid AppCheck token" };
                var result = JsonConvert.SerializeObject(responseModel);
                await context.Response.WriteAsync(result);
                return;
            }

            await _next(context);
        }

        private async Task<bool> IsValidAppCheckToken(string appCheckToken)
        {
            try
            {
                var keys = await GetFirebasePublicKeysAsync();
                var handler = new JwtSecurityTokenHandler();

                _logger.LogInformation("Reading provided JWT token");
                var token = handler.ReadJwtToken(appCheckToken);

                FirebaseApp firebaseApp = FirebaseApp.DefaultInstance;
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = $"https://firebaseappcheck.googleapis.com/{firebaseApp.Options.ProjectId}",
                    ValidateAudience = true,
                    ValidAudience = $"projects/{firebaseApp.Options.ProjectId}",
                    ValidateLifetime = true,
                    RequireSignedTokens = true,
                    IssuerSigningKeys = keys,
                    ValidateIssuerSigningKey = true,
                };

                ClaimsPrincipal principal = handler.ValidateToken(appCheckToken, validationParameters, out SecurityToken validatedToken);
                _logger.LogInformation("Give result of appcheck token");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Error verifying token: {ex.Message}");
                Console.WriteLine($"Error verifying token: {ex.Message}");
                return false;
            }
        }

        private async Task<IEnumerable<SecurityKey>> GetFirebasePublicKeysAsync()
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetStringAsync(_firebaseAppCheckJwksUrl);
                var jwks = JObject.Parse(response);
                var keys = jwks["keys"]
                    .Select(key => new JsonWebKey(key.ToString()))
                    .Cast<SecurityKey>();

                if (!keys.Any())
                {
                    Console.WriteLine("JWKS endpoint returned no keys.");
                }

                return keys;
            }
        }
    }
}
