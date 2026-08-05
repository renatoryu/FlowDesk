using FlowDesk.Api.ErrorHandling;
using FlowDesk.Application.Authentication.Login;
using FlowDesk.Application.Authentication.Refresh;
using FlowDesk.Application.Authentication.Register;
using FlowDesk.Application.Companies.Create;
using FlowDesk.Infrastructure;
using FluentValidation;
using Microsoft.OpenApi;
using FlowDesk.Api.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<
    IValidator<LoginUserCommand>,
    LoginUserCommandValidator>();

builder.Services.AddScoped<LoginUserHandler>();

builder.Services.AddScoped<
    IValidator<RefreshSessionCommand>,
    RefreshSessionCommandValidator>();

builder.Services.AddScoped<RefreshSessionHandler>();

builder.Services.AddScoped<
    IValidator<RegisterUserCommand>,
    RegisterUserCommandValidator>();

builder.Services.AddScoped<RegisterUserHandler>();

builder.Services.AddScoped<
    IValidator<CreateCompanyCommand>,
    CreateCompanyCommandValidator>();

builder.Services.AddScoped<CreateCompanyHandler>();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FlowDesk API",
        Version = "v1",
        Description =
            "API para gerenciamento de usuários, empresas e chamados."
    });

    options.AddSecurityDefinition(
        "bearer",
        new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description =
                "Informe somente o access token JWT."
        });

    options.AddSecurityRequirement(
        document => new OpenApiSecurityRequirement
        {
            [
                new OpenApiSecuritySchemeReference(
                    "bearer",
                    document)
            ] = []
        });

    options.OperationFilter<AuthenticationOperationFilter>();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "FlowDesk API v1");

        options.DocumentTitle = "FlowDesk API";
    });
}

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

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
