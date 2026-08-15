using BoxingGame.Domain.Match;
using BoxingGame.Networking;

var builder = WebApplication.CreateBuilder(args);
// Bind all interfaces for LAN play.
builder.WebHost.ConfigureKestrel(o => o.ListenAnyIP(5000));
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<MatchRegistry>();
builder.Services.AddHostedService<GameLoopService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
