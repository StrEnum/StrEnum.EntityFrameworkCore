FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY *.sln .
COPY src/StrEnum.EntityFrameworkCore/StrEnum.EntityFrameworkCore.csproj ./src/StrEnum.EntityFrameworkCore/StrEnum.EntityFrameworkCore.csproj
COPY test/StrEnum.EntityFrameworkCore.UnitTests/StrEnum.EntityFrameworkCore.UnitTests.csproj ./test/StrEnum.EntityFrameworkCore.UnitTests/StrEnum.EntityFrameworkCore.UnitTests.csproj
RUN dotnet restore

# copy everything else and build app
COPY ./ ./
WORKDIR /source
RUN dotnet build -c release --no-restore /p:maxcpucount=1

FROM build AS test
RUN dotnet test /p:maxcpucount=1

FROM build AS pack-and-push
WORKDIR /source

ARG PackageVersion
ARG NuGetApiKey

RUN dotnet pack ./src/StrEnum.EntityFrameworkCore/StrEnum.EntityFrameworkCore.csproj -o /out/package -c Release
RUN dotnet nuget push /out/package/StrEnum.EntityFrameworkCore.$PackageVersion.nupkg -k $NuGetApiKey -s https://api.nuget.org/v3/index.json