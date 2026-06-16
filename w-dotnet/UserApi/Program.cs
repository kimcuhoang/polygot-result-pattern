using System.Diagnostics;
using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using UserApi.Domains;
using UserApi.Features.Users.Create;
using UserApi.Features.Users.Find;
using UserApi.Infrastructures.EntityFramework;
using UserApi.Infrastructures.ExceptionHandlers;
using UserApi.Infrastructures.Extensions;
using UserApi.Infrastructures.MediatR;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
    options.Configure(builder.Configuration.GetSection("Kestrel"));
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    // Indent the output JSON for readability
    options.SerializerOptions.WriteIndented = true;
    // Ignore object cycles (prevent infinite reference loops)
    options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.AddDbContext<UserDbContext>((sp, options) =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("Default");

    options
    .UseSnakeCaseNamingConvention()
    .UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure();
        npgsqlOptions.CommandTimeout(30);
        npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory".ToSnakeCase());
    });

    options.UseSeeding((context, _) =>
    {
        var hasDefaultUser = context.Set<User>().Any(u => u.Id == User.DefaultUser.Id);
        if (!hasDefaultUser)
        {
            context.Set<User>().Add(User.DefaultUser);
            context.SaveChanges();
        }
    });

    options.UseAsyncSeeding(async (context, _, cancellationToken) =>
    {
        var hasDefaultUser = await context.Set<User>().AnyAsync(u => u.Id == User.DefaultUser.Id, cancellationToken);
        if (!hasDefaultUser)
        {
            context.Set<User>().Add(User.DefaultUser);
            await context.SaveChangesAsync(cancellationToken);
        }
    });
});

builder.Services
    .AddOpenApi("v1")
    .AddEndpointsApiExplorer()
    .AddHttpLogging()
    .AddProblemDetails()
    .AddMediatR(cfg =>
    {
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(RequestValidatorBehavior<,>));
        cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    })
    .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), includeInternalTypes: true)
    .AddExceptionHandler<ValidationExceptionHandler>()
    .AddExceptionHandler<UnexpectedExceptionHandler>()
    .AddValidation()
    .AddProblemDetails(options =>
    {
        options.CustomizeProblemDetails = context =>
        {
            context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
            context.ProblemDetails.Extensions.TryAdd("traceId", Activity.Current?.Id ?? context.HttpContext.TraceIdentifier);
        };
    });

var app = builder.Build();

app.UseExceptionHandler();
app.MapOpenApi();
app.MapScalarApiReference(opt =>
{
    opt.ForceDarkMode().ShowOperationId().SortTagsAlphabetically().SortOperationsByMethod();
});
app.UseHttpLogging();
app.UseRouting();

app.MapGet("/", () => TypedResults.Redirect("/scalar"));
app.MapCreateUserEndpoint();
app.MapFindUserEndpoint();

app.Run();