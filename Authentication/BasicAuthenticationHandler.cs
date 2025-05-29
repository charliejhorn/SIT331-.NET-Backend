using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using robot_controller_api.Persistence;
using robot_controller_api.Models;
using System.Security.Claims;

namespace robot_controller_api.Authentication;

public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IUserDataAccess _usersRepo;

    public BasicAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options, 
        ILoggerFactory logger, 
        UrlEncoder encoder, 
        ISystemClock clock) 
        : base(options, logger, encoder, clock)
    { 
        _usersRepo = new UserRepository();
    }
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        base.Response.Headers.Append("WWW-Authenticate", @"Basic realm=""Access to the robot controller.""");

        // check if Authorization header exists
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            // no credentials provided. continue, Authorization middleware might deny later if endpoint requires auth
            // (this allows access to /swagger webpage, amoung other things)
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        // get endpoint metadata
        EndpointMetadataCollection? metadata = Context.GetEndpoint().Metadata;
        if (metadata == null)
        {
            Response.StatusCode = 401;
            return Task.FromResult(AuthenticateResult.Fail("No endpoint metadata found"));
        }

        // if endpoint has attribute AllowAnonymous, then return success
        var allowAnonymousMetadata = metadata.GetMetadata<IAllowAnonymous>();
        if (allowAnonymousMetadata != null)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        // get header details
        var authHeader = base.Request.Headers["Authorization"].ToString();

        if(string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Basic "))
        {
            Response.StatusCode = 401;
            return Task.FromResult(AuthenticateResult.Fail("No authentication header found"));
        }

        // get base64 string from authHeader without the Basic part of the Authorization HTTP header
        var base64String = authHeader.Substring("Basic ".Length).Trim();

        // decode the base64 string
        var decodedBytes = Convert.FromBase64String(base64String);
        var decodedString = System.Text.Encoding.UTF8.GetString(decodedBytes);
        
        // split the string into email and password
        var credentials = decodedString.Split(':');
        var email = credentials[0];
        var password = credentials[1];

        // check if the user exists in the database
        UserModel? user = _usersRepo.GetUsers().FirstOrDefault(user => user.Email == email);
        
        if(user == null)
        {
            Response.StatusCode = 401;
            return Task.FromResult(AuthenticateResult.Fail("Authentication failed: no user with that email exists."));
        }

        // check if the password is correct
        var pwVerificationResult = PasswordService.VerifyPassword(password, user.PasswordHash);
        if(pwVerificationResult == PasswordVerificationResult.Success)
        {
            // create claims and identity
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Role, user.Role ?? "User"),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };

            // create claims principal and authentication ticket, and return success
            ClaimsIdentity identity = new ClaimsIdentity(claims, "Basic");
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);
            AuthenticationTicket authTicket = new AuthenticationTicket(claimsPrincipal, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(authTicket));
        }

        // by default, return unauthorized
        Response.StatusCode = 401;
        return Task.FromResult(AuthenticateResult.Fail("Authentication failed."));
    }
}