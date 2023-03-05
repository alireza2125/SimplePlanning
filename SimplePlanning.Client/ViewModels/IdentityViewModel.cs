using System.Net.Http.Json;

using Microsoft.AspNetCore.Components;

using MudBlazor;

using SimplePlanning.Client.Services;
using SimplePlanning.Shared.Models;

namespace SimplePlanning.Client.ViewModels;

public class IdentityViewModel
{
    private readonly HttpClient _httpClient;
    private readonly IdentityService _identityService;
    private readonly ISnackbar _snackbar;
    private readonly NavigationManager _navigationManager;

    public IdentityLoginRequest Model { get; set; }

    public IdentityViewModel(HttpClient httpClient,
        IdentityService identityService,
        ISnackbar Snackbar,
        NavigationManager navigationManager)
    {
        _httpClient = httpClient;
        _identityService = identityService;
        _snackbar = Snackbar;
        _navigationManager = navigationManager;
        Model = new(_identityService.UserModel?.Email ?? string.Empty,
            _identityService.UserModel?.Password ?? string.Empty);
    }

    public async ValueTask LoginAsync()
    {
        using var loginRequest = new HttpRequestMessage(HttpMethod.Get, "/api/identity");
        loginRequest.Headers.SetAuthorization(Model.GetToken());
        using var loginResponse = await _httpClient.SendAsync(loginRequest).ConfigureAwait(false);
        if (loginResponse.IsSuccessStatusCode)
        {
            var userModel = await loginResponse.Content.ReadFromJsonAsync<UserModel>().ConfigureAwait(false);
            if (userModel != null)
            {
                userModel.Password = Model.Password;
                await _identityService.SetUserAsync(userModel).ConfigureAwait(false);
                _snackbar.Add("Login successful", Severity.Success);
                _navigationManager.NavigateTo("/");
                return;
            }
        }

        _snackbar.Add("Login failed", Severity.Error);
        _navigationManager.NavigateTo("/");
    }

    public async ValueTask SignupAsync()
    {
        using var response =
            await _httpClient.PostAsJsonAsync("/api/identity", Model).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            _snackbar.Add("Signup failed", Severity.Error);
            return;
        }

        var userModel = await response.Content.ReadFromJsonAsync<UserModel>().ConfigureAwait(false);

        if (userModel is null)
        {
            _snackbar.Add("Signup failed", Severity.Error);
            return;
        }

        userModel.Password = Model.Password;
        await _identityService.SetUserAsync(userModel).ConfigureAwait(false);
        _snackbar.Add("Signup successful and auto login", Severity.Error);
        _navigationManager.NavigateTo("/");
    }
}
