using System.Diagnostics;
using System.Reflection;
using FluentValidation;
using MediatR;
using Scalar.AspNetCore;
using UserApi.Features.Users.Create;
using UserApi.Features.Users.Find;
using UserApi.Infrastructures.ExceptionHandlers;
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