var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRouting();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapFallbackToFile("index.html");

app.Run();
