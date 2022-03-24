﻿using Ayedroid.Poker.App.Exceptions;
using Ayedroid.Poker.App.Hubs;
using Ayedroid.Poker.App.Interfaces;
using Ayedroid.Poker.App.Models;
using Ayedroid.Poker.App.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;

const string expiredTokenHeader = "Token-Expired";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
                      builder =>
                      {
                          builder.WithOrigins("http://localhost:4200/");
                      });
});

TokenAuthOptions tokenAuthOptions = new()
{
    Audience = builder.Configuration["Jwt:Audience"],
    Issuer = builder.Configuration["Jwt:Issuer"]

};
builder.Services.AddSingleton<TokenAuthOptions>(tokenAuthOptions);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        IssuerSigningKey = tokenAuthOptions.Key,
        ValidAudience = tokenAuthOptions.Audience,
        ValidIssuer = tokenAuthOptions.Issuer,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(0)
    };
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
         {
             if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
             {
                 context.Response.Headers.Add(expiredTokenHeader, "True");
                 context.Response.StatusCode = 401;
             }
             return Task.CompletedTask;
         }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme‌​)
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddControllers(o =>
{
    // Without this any Ok(string) will be sent back as text/plain - which angular doesn't like
    o.OutputFormatters.RemoveType<StringOutputFormatter>();
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

builder.Services.AddSingleton<INotificationService>(serviceProvider =>
new NotificationService(
    serviceProvider.GetRequiredService<ILogger<NotificationService>>(),
    serviceProvider.GetRequiredService<IHubContext<NotificationHub, INotificationClient>>()
));
builder.Services.AddSingleton<IUserService>(serviceProvider =>
    new UserService(
        serviceProvider.GetRequiredService<ILogger<UserService>>(),
        serviceProvider.GetRequiredService<INotificationService>()

));
builder.Services.AddSingleton<ISessionService>(serviceProvider =>
    new SessionService(
        serviceProvider.GetRequiredService<ILogger<SessionService>>(),
        serviceProvider.GetRequiredService<INotificationService>(),
        serviceProvider.GetRequiredService<IUserService>()
));

builder.Services.AddSingleton<ITokenService>(serviceProvider =>
    new TokenService(
        serviceProvider.GetRequiredService<ILogger<TokenService>>(),
        serviceProvider.GetRequiredService<TokenAuthOptions>()
));

var app = builder.Build();

app.UseExceptionHandler(a => a.Run(async context =>
{
    IExceptionHandlerPathFeature? exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
    Exception? exception = exceptionHandlerPathFeature?.Error;

    int statusCode = 500;
    string error = "Unknown error";
    // use default if null
    if (exception is null) { }
    else if (exception is SessionNotFoundException) { statusCode = StatusCodes.Status404NotFound; error = "Session does not exist"; }
    else if (exception is ArgumentNullException) { statusCode = StatusCodes.Status400BadRequest; error = "All fields must have a value"; }

    context.Response.StatusCode = statusCode;
    await context.Response.WriteAsJsonAsync(new { error });
}));

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors((o) =>
   o.AllowAnyOrigin()
  .AllowAnyMethod()
  .AllowAnyHeader()
  .WithExposedHeaders(expiredTokenHeader)
);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
