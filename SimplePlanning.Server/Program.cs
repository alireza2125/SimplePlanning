using Microsoft.EntityFrameworkCore;

using SimplePlanning.Server.Data;
using SimplePlanning.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("mssql")));

builder.Services.AddScoped<IdentityService>();
builder.Services.AddAuthentication()
    .AddScheme<IdentityOptions, IdentityHandler>(IdentityHandler.SchemeName, _ => { });
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles(new StaticFileOptions()
{
    ServeUnknownFileTypes = true
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html");

{
    var serviceScope = app.Services.CreateAsyncScope();
    await using var _ = serviceScope.ConfigureAwait(false);
    await serviceScope.ServiceProvider.GetRequiredService<DataContext>()
        .Database.MigrateAsync()
        .ConfigureAwait(false);
}

await app.RunAsync().ConfigureAwait(false);
