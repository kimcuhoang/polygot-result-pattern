#:sdk Aspire.AppHost.Sdk@13.4.3
#pragma warning disable ASPIRECSHARPAPPS001

var builder = DistributedApplication.CreateBuilder(args);

var csharpApp = builder.AddCSharpApp("CSharpApp", "../w-dotnet/UserApi/UserApi.csproj");

builder.Build().Run();
