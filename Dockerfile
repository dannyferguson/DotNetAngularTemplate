# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS builder

# Install Node.js (v20)
RUN apt-get update && \
    apt-get install -y curl && \
    curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get update && \
    apt-get install -y nodejs

WORKDIR /app

# Copy everything
COPY . .

# Run Nuke build
RUN ./build.sh --target Publish || ./build.cmd --target Publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Copy published app from NUKE build output
COPY --from=builder /app/Output .

EXPOSE 5000

ENTRYPOINT ["dotnet", "DotNetAngularTemplate.dll"]