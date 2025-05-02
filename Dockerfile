# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS builder

# Install Node.js (v20)
RUN apt-get update && \
    apt-get install -y curl && \
    curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get update && \
    apt-get install -y nodejs dos2unix

WORKDIR /app

# Copy everything
COPY . .

# Run Nuke build
RUN dos2unix ./build.sh && ./build.sh --target Publish --root /app

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Copy published app from NUKE build output
COPY --from=builder /app/Output .

EXPOSE 5000

ENTRYPOINT ["dotnet", "DotNetAngularTemplate.dll"]