default:
    just --list

dnt command package:
    dotnet tool {{command}} {{package}}

# The aspire new or init commands is now not working with windows
# they can extract the bundle successfully but fail at validate layout
# the following commands will be used to setup this app after they fix for windows

# ====================
# dotnet aspire new aspire-empty -n polygot --language csharp --non-interactive --suppress-agent-init

aspire command *options:
    clear
    dotnet aspire {{command}} {{options}}

run:
    clear
    dotnet aspire run
    # dotnet run apphost.cs --no-launch-profile