
default:
    just --list

dotnet-tool-install:
    dotnet tool install --local dotnet-ef
    dotnet tool install --local centralisedpackageconverter
    dotnet tool install --local dotnet-outdated-tool

dotnet-tools-update:
    dotnet tool update --all

update-outdated-packages:
    dotnet outdated -u

add-readme:
    echo "Creating README.md..."
    echo '# UserApi' > README.md
    echo '' >> README.md
    echo 'A modern Minimal API built with .NET 10.' >> README.md
    echo '' >> README.md
    echo '## Features' >> README.md
    echo '- **.NET 10.0.300** architecture via `global.json`' >> README.md
    echo '- Streamlined **`.slnx` solution** format' >> README.md
    echo '- **Central Package Management (CPM)** via `Directory.Packages.props`' >> README.md
    echo '- Global build configurations via `Directory.Build.props`' >> README.md
    echo '' >> README.md
    echo '## How to Run' >> README.md
    echo '```bash' >> README.md
    echo 'dotnet restore' >> README.md
    echo 'dotnet run --project UserApi/UserApi.csproj' >> README.md

start-initialization:
    dotnet new gitignore --force
    dotnet new globaljson --sdk-version 10.0.301 --roll-forward patch --force
    dotnet new buildprops --force
    dotnet new sln --name UserApi --force
    dotnet new webapi -minimal --no-https --no-restore --force -o UserApi
    dotnet sln UserApi.slnx add UserApi/UserApi.csproj

add-central-packages-management:
    dotnet new tool-manifest --force
    dotnet tool install --local CentralisedPackageConverter
    dotnet tool run central-pkg-converter ./ -f

verify:
    dotnet clean
    dotnet restore --force
    dotnet build

init: start-initialization add-central-packages-management add-readme verify
    echo "🚀 Solution successfully initialized with UserApi.slnx!"

reset:
    echo "Cleaning up generated files..."
    {{ if os() == "windows" { \
        "powershell -Command \" \
            if (Test-Path UserApi) { Remove-Item -Recurse -Force UserApi }; \
            Get-ChildItem -Path . -File -Recurse | Where-Object { $_.Name -ne 'justfile' -and $_.Name -ne '.justfile' } | Remove-Item -Force \
        \"" \
    } else { \
        "rm -rf UserApi && \
         find . -type f ! -name '.justfile' ! -name 'justfile' -delete" \
    } }}
    @echo "🧹 Workspace cleaned."


build:
    dotnet clean
    dotnet build

run: build
    dotnet run --no-launch-profile --project UserApi/UserApi.csproj

ef-add name:
    dotnet ef migrations add {{name}} \
        --project UserApi/UserApi.csproj \
        --startup-project UserApi/UserApi.csproj \
        -c UserDbContext \
        --output-dir Infrastructures/EntityFramework/Migrations

ef-bundle-generate:
    dotnet ef migrations bundle \
        --project UserApi/UserApi.csproj \
        --startup-project UserApi/UserApi.csproj \
        -c UserDbContext \
        -o deploy/userdb-postgres.exe --force

ef-bundle-run: ef-bundle-generate
    clear
    deploy/userdb-postgres.exe --verbose --connection "Host=localhost;Port=5432;Database=dotnet-result-pattern;Username=postgres;Password=postgres"
