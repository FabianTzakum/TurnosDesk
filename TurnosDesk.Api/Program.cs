using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TurnosDesk.Api.Data;
using TurnosDesk.Api.Hubs;
using TurnosDesk.Api.Infrastructure.Seed;
using TurnosDesk.Api.Services.Implementations;
using TurnosDesk.Api.Services.Interfaces;
using TurnosDesk.Api.Support.Errors;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers(options =>
    {
        options.Filters.Add<ApiExceptionFilter>();
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = ValidationErrorResponseFactory.CreateResponse;
});

builder.Services.AddDbContext<TurnosDeskDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.PayloadSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddScoped<IBranchService, BranchService>();
builder.Services.AddScoped<IServiceAreaService, ServiceAreaService>();
builder.Services.AddScoped<IServiceModuleService, ServiceModuleService>();
builder.Services.AddScoped<IServiceTypeService, ServiceTypeService>();
builder.Services.AddScoped<IQueueTicketService, QueueTicketService>();
builder.Services.AddScoped<IQueueAttentionService, QueueAttentionService>();
builder.Services.AddScoped<ITicketEventService, TicketEventService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ISystemCatalogService, SystemCatalogService>();
builder.Services.AddScoped<IQueueRealtimeNotifier, QueueRealtimeNotifier>();

builder.Services.AddScoped<TurnosDeskSeeder>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("TurnosDeskCors", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed(_ => true);
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<TurnosDeskSeeder>();
    await seeder.SeedAsync();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.DocumentTitle = "TurnosDesk API";
        options.DisplayRequestDuration();
        options.EnableTryItOutByDefault();
    });
}

app.UseHttpsRedirection();

app.UseCors("TurnosDeskCors");

app.UseAuthorization();

app.MapControllers();

app.MapHub<QueueHub>("/hubs/queue");

app.Run();
