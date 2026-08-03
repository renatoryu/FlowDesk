using FlowDesk.Api.ErrorHandling;
using FlowDesk.Application.Authentication.Register;
using FlowDesk.Infrastructure;
using FluentValidation;
using FlowDesk.Application.Authentication.Login;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<
    IValidator<LoginUserCommand>,
    LoginUserCommandValidator>();

builder.Services.AddScoped<LoginUserHandler>();

builder.Services.AddScoped<
    IValidator<RegisterUserCommand>,
    RegisterUserCommandValidator>();

builder.Services.AddScoped<RegisterUserHandler>();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddControllers();

var app = builder.Build();

app.UseExceptionHandler();

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
