var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
    application = "FlowDesk.Api",
    status = "running"
}));

app.MapControllers();

app.Run();

public partial class Program
{
}
