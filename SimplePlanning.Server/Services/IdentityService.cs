using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using SimplePlanning.Server.Data;
using SimplePlanning.Shared.Models;

namespace SimplePlanning.Server.Services;

public class IdentityOptions : AuthenticationSchemeOptions
{
}

public class IdentityHandler : AuthenticationHandler<IdentityOptions>
{
    public const string SchemeName = "Basic";
    private readonly IdentityService _identityService;

    public IdentityHandler(
        IdentityService identityService,
        IOptionsMonitor<IdentityOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock) : base(options, logger, encoder, clock) => _identityService = identityService;

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
            var bytes = new byte[64];
            return AuthenticationHeaderValue.TryParse(Context.Request.Headers.Authorization,
                       out var authenticationHeaderValue) &&
                   !string.IsNullOrWhiteSpace(authenticationHeaderValue.Parameter) &&
                   Convert.TryFromBase64String(authenticationHeaderValue.Parameter, new(bytes), out var bytesWritten) &&
                   Encoding.UTF8.GetString(bytes[..bytesWritten]) is { } emailPasswordString &&
                   emailPasswordString.Split(':') is { } emailPassword &&
                   await _identityService.LoadAsync(emailPassword[0], emailPassword[1]).ConfigureAwait(false) is
                       { } authenticationTicket
                ? AuthenticateResult.Success(authenticationTicket)
                : AuthenticateResult.NoResult();
        }
        catch (Exception ex)
        {
            return AuthenticateResult.Fail(ex);
        }
    }
}

public class IdentityService
{
    private readonly DataContext _dataContext;

    public UserModel? User { get; private set; }

    public IdentityService(DataContext dataContext) => _dataContext = dataContext;

    public async ValueTask<AuthenticationTicket?> LoadAsync(string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        User = await _dataContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == email && x.Password == password, cancellationToken)
            .ConfigureAwait(false);

        return User is null
            ? null
            : new AuthenticationTicket(new(new ClaimsIdentity(
                    Enumerable.Empty<Claim>().Append(new(ClaimTypes.Email, User!.Email)),
                    email)),
                IdentityHandler.SchemeName);
    }
}
