using ApexCharts;
using Application;
using Application.Common.Interfaces;
using Application.DTOs;
using Application.Features.Traps.Commands;
using Application.Features.Traps.Queries;
using Application.Settings;
using Infrastructure;
using Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Presentation.Components;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApexCharts();
builder.Services.AddTransient<MediatR.Mediator>();
builder.Services.AddScoped<Presentation.Services.ScopedMediator>();
builder.Services.AddScoped<IMediator>(provider => provider.GetRequiredService<Presentation.Services.ScopedMediator>());
builder.Services.AddScoped<ISender>(provider => provider.GetRequiredService<Presentation.Services.ScopedMediator>());
builder.Services.AddScoped<IPublisher>(provider => provider.GetRequiredService<Presentation.Services.ScopedMediator>());
// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<TrapSettings>(builder.Configuration.GetSection("TrapSettings"));
builder.Services.AddAuthorization();

var app = builder.Build();

// Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TrapsSystem API V1");
        c.RoutePrefix = "swagger";
      //  c.EnableAnnotations();  
    });
}

// Production
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

// Middleware
//app.UseHttpsRedirection();

app.UseRouting();

// app.UseAuthentication();

app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();

// Endpoints
app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
