using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

using SimplePlanning.Shared.Models;

namespace SimplePlanning.Client.Services;

public class IdentityService : AuthenticationStateProvider
{
    private readonly IJSRuntime _jSRuntime;
    private readonly HttpClient _httpClient;
    private readonly ILogger<IdentityService> _logger;
    public const string TokenKey = "token";

    public UserModel? UserModel { get; set; }

    public IdentityService(IJSRuntime jSRuntime, HttpClient httpClient, ILogger<IdentityService> logger)
    {
        _jSRuntime = jSRuntime;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async ValueTask SetUserAsync(UserModel user)
    {
        UserModel = user;
        await _jSRuntime.InvokeVoidAsync("localStorage.setItem", TokenKey, user.GetToken()).ConfigureAwait(false);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async ValueTask<UserModel?> GetUserAsync()
    {
        var token = await _jSRuntime.InvokeAsync<string>("localStorage.getItem", TokenKey).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        _httpClient.DefaultRequestHeaders.SetAuthorization(token);
        return UserModel = await _httpClient.GetFromJsonAsync<UserModel>("/api/identity").ConfigureAwait(false);
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            return await GetUserAsync().ConfigureAwait(false) is { } userModel
                ? new(new(new ClaimsIdentity(
                    Enumerable.Empty<Claim>()
                        .Append(new(ClaimTypes.Email, userModel.Email)),
                    userModel.Email)))
                : new AuthenticationState(new());
        }
        catch (Exception ex)
        {
#pragma warning disable CA1848
            _logger.LogError(ex, "error on auth");
#pragma warning restore CA1848
            return new(new());
        }
    }
}

public static class IdentityExtensions
{
    public static string GetToken(this UserModel model) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes($"{model.Email}:{model.Password}"));

    public static string GetToken(this IdentityLoginRequest model) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes($"{model.Email}:{model.Password}"));

    public static void SetAuthorization(this HttpRequestHeaders keyValuePairs, string token) =>
        keyValuePairs.Authorization = new("Basic", token);

    public static void SetAuthorization(this HttpRequestHeaders keyValuePairs, UserModel userModel) =>
        keyValuePairs.SetAuthorization(userModel.GetToken());
}
