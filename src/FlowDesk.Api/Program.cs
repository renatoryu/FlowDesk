using FlowDesk.Api.Authorization;
using FlowDesk.Api.ErrorHandling;
using FlowDesk.Api.OpenApi;
using FlowDesk.Api.Security;
using FlowDesk.Application.Abstractions.Security;
using FlowDesk.Application.Authentication.Login;
using FlowDesk.Application.Authentication.Refresh;
using FlowDesk.Application.Authentication.Register;
using FlowDesk.Application.Categories.List;
using FlowDesk.Application.Comments.Create;
using FlowDesk.Application.Comments.List;
using FlowDesk.Application.Companies.Create;
using FlowDesk.Application.Companies.Deactivate;
using FlowDesk.Application.Companies.GetById;
using FlowDesk.Application.Companies.List;
using FlowDesk.Application.Companies.Update;
using FlowDesk.Application.Tickets.ChangeStatus;
using FlowDesk.Application.Tickets.Create;
using FlowDesk.Application.Tickets.Delete;
using FlowDesk.Application.Tickets.GetById;
using FlowDesk.Application.Tickets.List;
using FlowDesk.Application.Tickets.Update;
using FlowDesk.Application.Users.AssignCompany;
using FlowDesk.Domain.Enums;
using FlowDesk.Infrastructure;
using FluentValidation;
using Microsoft.OpenApi;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<
    ICurrentUser,
    HttpContextCurrentUser>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        AuthorizationPolicies.CompanyRead,
        policy => policy.RequireRole(
            nameof(UserRole.Admin),
            nameof(UserRole.Agent)));

    options.AddPolicy(
        AuthorizationPolicies.CompanyWrite,
        policy => policy.RequireRole(
            nameof(UserRole.Admin)));

    options.AddPolicy(
        AuthorizationPolicies.UserCompanyWrite,
        policy => policy.RequireRole(
            nameof(UserRole.Admin)));

    options.AddPolicy(
        AuthorizationPolicies.TicketCreate,
        policy => policy.RequireRole(
            nameof(UserRole.Customer)));

    options.AddPolicy(
        AuthorizationPolicies.TicketRead,
        policy => policy.RequireRole(
            nameof(UserRole.Customer),
            nameof(UserRole.Agent),
            nameof(UserRole.Admin)));

    options.AddPolicy(
        AuthorizationPolicies.TicketUpdate,
        policy => policy.RequireRole(
            nameof(UserRole.Customer),
            nameof(UserRole.Agent),
            nameof(UserRole.Admin)));
    options.AddPolicy(
    AuthorizationPolicies.TicketStatusChange,
        policy => policy.RequireRole(
            nameof(UserRole.Customer),
            nameof(UserRole.Agent),
            nameof(UserRole.Admin)));
    options.AddPolicy(
        AuthorizationPolicies.TicketDelete,
        policy => policy.RequireRole(
            nameof(UserRole.Customer),
            nameof(UserRole.Agent),
            nameof(UserRole.Admin)));
    options.AddPolicy(
        AuthorizationPolicies.CommentCreate,
        policy => policy.RequireRole(
            nameof(UserRole.Customer),
            nameof(UserRole.Agent),
            nameof(UserRole.Admin)));

    options.AddPolicy(
        AuthorizationPolicies.CommentRead,
        policy => policy.RequireRole(
            nameof(UserRole.Customer),
            nameof(UserRole.Agent),
            nameof(UserRole.Admin)));
});

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

builder.Services.AddScoped<
    IValidator<UpdateCompanyCommand>,
    UpdateCompanyCommandValidator>();

builder.Services.AddScoped<
    IValidator<AssignUserCompanyCommand>,
    AssignUserCompanyCommandValidator>();

builder.Services.AddScoped<
    IValidator<CreateTicketCommand>,
    CreateTicketCommandValidator>();

builder.Services.AddScoped<
    IValidator<ListTicketsQuery>,
    ListTicketsQueryValidator>();

builder.Services.AddScoped<
    IValidator<GetTicketByIdQuery>,
    GetTicketByIdQueryValidator>();

builder.Services.AddScoped<
    IValidator<UpdateTicketCommand>,
    UpdateTicketCommandValidator>();

builder.Services.AddScoped<
    IValidator<ChangeTicketStatusCommand>,
    ChangeTicketStatusCommandValidator>();

builder.Services.AddScoped<
    IValidator<DeleteTicketCommand>,
    DeleteTicketCommandValidator>();

builder.Services.AddScoped<
    IValidator<CreateCommentCommand>,
    CreateCommentCommandValidator>();

builder.Services.AddScoped<
    IValidator<ListTicketCommentsQuery>,
    ListTicketCommentsQueryValidator>();

builder.Services.AddScoped<AssignUserCompanyHandler>();
builder.Services.AddScoped<ListCategoriesHandler>();
builder.Services.AddScoped<UpdateCompanyHandler>();
builder.Services.AddScoped<CreateCompanyHandler>();
builder.Services.AddScoped<GetCompanyByIdHandler>();
builder.Services.AddScoped<ListCompaniesHandler>();
builder.Services.AddScoped<DeactivateCompanyHandler>();
builder.Services.AddScoped<CreateTicketHandler>();
builder.Services.AddScoped<ListTicketsHandler>();
builder.Services.AddScoped<GetTicketByIdHandler>();
builder.Services.AddScoped<UpdateTicketHandler>();
builder.Services.AddScoped<ChangeTicketStatusHandler>();
builder.Services.AddScoped<DeleteTicketHandler>();
builder.Services.AddScoped<CreateCommentHandler>();
builder.Services.AddScoped<ListTicketCommentsHandler>();

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
