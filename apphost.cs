#:sdk Aspire.AppHost.Sdk@13.4.3
#pragma warning disable ASPIRECSHARPAPPS001

// #:package Aspire.Hosting.JavaScript@13.4.3
// #:package CommunityToolkit.Aspire.Hosting.NodeJS.Extensions@9.9.0

using Aspire.Hosting;
using Microsoft.Extensions.Configuration;

Environment.SetEnvironmentVariable("ASPIRE_DCP_USE_DEVELOPER_CERTIFICATE", "false");
Environment.SetEnvironmentVariable("ASPIRE_ALLOW_UNSECURED_TRANSPORT", "true");

var options = new DistributedApplicationOptions
{
    Args = args,
    AllowUnsecuredTransport = true,       // Bypasses HTTP restrictions natively
    TrustDeveloperCertificate = false     // Explicitly disables searching for/validating trusted local dev-certs

};

var builder = DistributedApplication.CreateBuilder(options);

// 1. Force the AppHost and Dashboard to run entirely on HTTP without launchSettings
builder.Configuration["ASPIRE_DASHBOARD_OTLP_ENDPOINT_URL"] = "http://localhost:18889";
builder.Configuration["ASPIRE_APP_HOST_OTLP_ENDPOINT_URL"] = "http://localhost:18889";
builder.Configuration["ASPNETCORE_URLS"] = "http://localhost:18888";

// 2. Configure dashboard without token
builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    ["AppHost:BrowserToken"] = ""
});

var csharpApp = builder
    .AddCSharpApp("CSharpApp", "./w-dotnet/UserApi/UserApi.csproj")
    .WithHttpEndpoint(port: 5001);

var nestjsApp = builder
    .AddExecutable(name: "nestjs-api",
        command: "bash",
        workingDirectory: "./w-nestjs",
        args: ["-c", "pwd && source ~/.nvm/nvm.sh && nvm use && yarn install && yarn run start:dev"]
        // args: ["-l", "-c", "pwd && nvm use && yarn install && yarn run start:dev"]
    )
    .WithHttpEndpoint(port: 3000, targetPort: 3000, name: "http", isProxied: false)
    .WithExternalHttpEndpoints();


// var nestjsApp = builder
//     .AddYarnApp("NestJsApp", workingDirectory: "./w-nestjs", scriptName: "start:dev")
//     .WithHttpEndpoint(port: 3000);

// builder.AddNodeApp("NestJS", workingDirectory: "./w-nestjs", scriptPath: "src/main.ts")
//     .WithHttpEndpoint(port: 3000)
//     .WithYarnPackageInstallation(config =>
//     {
//     });


// string shellExec;
// string[] shellArgs;

// if (OperatingSystem.IsWindows())
// {
//     shellExec = "cmd.exe";
//     // Windows: /c runs the string command. nvm-windows updates the symlink, then we execute yarn.
//     shellArgs = ["/c", "nvm use && yarn install && yarn run start:dev"];
// }
// else
// {
//     shellExec = "bash";
//     // Linux/macOS: -l runs a login shell to source nvm profiles properly.
//     shellArgs = ["-l", "-c", "nvm use && yarn install && yarn run start:dev"];
// }
// var nestApi = builder.AddExecutable(
//     name: "nestjs-api",
//     command: shellExec,
//     workingDirectory: "../your-nestjs-project-folder",
//     args: shellArgs
// )
// .WithHttpEndpoint(port: 3000, targetPort: 3000, name: "http", isProxied: false)
// .WithExternalHttpEndpoints();
    

builder.Build().Run();