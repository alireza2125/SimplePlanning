using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.EntityFrameworkCore;

using MudBlazor.Services;

using SimplePlanning.Client;
using SimplePlanning.Client.Data;
using SimplePlanning.Client.Services;
using SimplePlanning.Client.ViewModels;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(_ => new HttpClient
{
    BaseAddress = new(builder.HostEnvironment.BaseAddress)
});

builder.Services.AddDbContext<DataContext>(options => options.UseInMemoryDatabase("mdb"));

builder.Services.AddOptions();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<IdentityService>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<IdentityService>());

builder.Services.AddScoped<IdentityViewModel>();
builder.Services.AddScoped<PlaningViewModel>();

builder.Services.AddMudServices();

var app = builder.Build();

await app.RunAsync();
