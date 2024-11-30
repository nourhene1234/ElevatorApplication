using crudmongo.Configurations;
using crudmongo.Services;
using FirstWebApp.Domaine.services;
using FirstWebApp.Infra.ServicesImp;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection(key: "MongoDatabase"));
builder.Services.AddSingleton<ElevatorService>();
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<IMqttClientService, MqttClientService>();
builder.Services.AddSingleton<JwtTokenService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var key = Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]);
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddAuthorization();


var app = builder.Build();


// Configurer WebSockets
app.UseWebSockets();

var mqttClientService = app.Services.GetRequiredService<IMqttClientService>();
await mqttClientService.ConnectAsync();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapGet("/", () => "Bienvenue sur le serveur WebSocket!");

app.Map("/ws", async (HttpContext context, CancellationToken cancellationToken, ElevatorService statusService, IMqttClientService mqttClientService) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var socket = await context.WebSockets.AcceptWebSocketAsync();

        // Créer le service WebSocket avec l'instance IMqttClientService
        var webSocketHandler = new WebSocketHandler(socket);
        var webSocketService = new WebSocketService(webSocketHandler, statusService, mqttClientService);

        // Traiter la connexion WebSocket
        await webSocketService.HandleWebSocketConnectionAsync(socket, cancellationToken);
    }
    else
    {
        context.Response.StatusCode = 400;  // Si ce n'est pas une requête WebSocket
    }
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
