using Microsoft.AspNetCore.Http;
using SpotifyAPI.Web;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string clientId = ""; //ClintId Girin
string clientSecret = ""; //ClintSecret Girin
string redirectUri = ""; // Redirect Uri
string accessToken = ""; // Erişim token'ını saklamak için

// Yetkilendirme isteği oluşturma
app.MapGet("/", async context =>
{
    if (string.IsNullOrEmpty(accessToken))
    {
        // Kullanıcı oturum açmamışsa giriş isteği oluştur
        var loginRequest = new LoginRequest(
            new Uri(redirectUri),
            clientId,
            LoginRequest.ResponseType.Code
        )
        {
            Scope = new[] { Scopes.UserReadCurrentlyPlaying, Scopes.UserReadPlaybackState }
        };

        var uri = loginRequest.ToUri();
        context.Response.Redirect(uri.ToString());
        return; // Bu noktadan sonra işlem yapma
    }

    // Kullanıcı giriş yapmışsa, çalan şarkıyı göster
    var spotify = new SpotifyClient(accessToken);
    var currentlyPlaying = await spotify.Player.GetCurrentlyPlaying(new PlayerCurrentlyPlayingRequest());

    if (currentlyPlaying?.Item is FullTrack track)
    {
        await context.Response.WriteAsync($"Suan Calan Sarki:\n{track.Artists[0].Name} - {track.Name}");
    }
    else
    {
        await context.Response.WriteAsync("Suan Calan Sarki Yok.");
    }
});

// Geri dönüş URI'si
app.MapGet("/callback", async context =>
{
    var code = context.Request.Query["code"].ToString();

    // Token almak için Authorization Code'u kullan
    var tokenRequest = new AuthorizationCodeTokenRequest(clientId, clientSecret, code, new Uri(redirectUri));
    var oauthClient = new OAuthClient();
    var tokenResponse = await oauthClient.RequestToken(tokenRequest);

    // Erişim token'ını sakla
    accessToken = tokenResponse.AccessToken;

    // Kullanıcı giriş yaptıktan sonra anasayfaya yönlendir
    context.Response.Redirect("/");
});

// Uygulamayı çalıştır
app.Run();
