# Build is ran to build the modules, since when run is called, it only builds the referenced project and dependencies.
dotnet build --no-restore && dotnet run --project Kernel --no-build --no-restore
