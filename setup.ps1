Set-Location C:\Users\User\Desktop\work\ContentGeneratorApp\backend
dotnet new sln -n ContentGenerator
dotnet new webapi -n ContentGenerator.Api
dotnet new classlib -n ContentGenerator.Domain
dotnet new classlib -n ContentGenerator.Application
dotnet new classlib -n ContentGenerator.Infrastructure

dotnet sln ContentGenerator.sln add ContentGenerator.Api/ContentGenerator.Api.csproj
dotnet sln ContentGenerator.sln add ContentGenerator.Domain/ContentGenerator.Domain.csproj
dotnet sln ContentGenerator.sln add ContentGenerator.Application/ContentGenerator.Application.csproj
dotnet sln ContentGenerator.sln add ContentGenerator.Infrastructure/ContentGenerator.Infrastructure.csproj

dotnet add ContentGenerator.Api/ContentGenerator.Api.csproj reference ContentGenerator.Application/ContentGenerator.Application.csproj
dotnet add ContentGenerator.Api/ContentGenerator.Api.csproj reference ContentGenerator.Infrastructure/ContentGenerator.Infrastructure.csproj
dotnet add ContentGenerator.Application/ContentGenerator.Application.csproj reference ContentGenerator.Domain/ContentGenerator.Domain.csproj
dotnet add ContentGenerator.Infrastructure/ContentGenerator.Infrastructure.csproj reference ContentGenerator.Application/ContentGenerator.Application.csproj
