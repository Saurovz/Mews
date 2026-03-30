FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /source

# required build args
ARG FEED_ACCESSTOKEN
ARG PACKAGE_VERSION
ARG GENERATE_BUNDLE=true
# validate required build args
RUN if [ -z "${FEED_ACCESSTOKEN}" ]; then echo "Error: FEED_ACCESSTOKEN is not set"; exit 1; fi
RUN if [ -z "${PACKAGE_VERSION}" ]; then echo "Error: PACKAGE_VERSION is not set"; exit 1; fi

# Install credentials provider and register custom feed
RUN <<eot bash
  apt-get update
  apt-get install -y curl
  curl -L https://raw.githubusercontent.com/Microsoft/artifacts-credprovider/master/helpers/installcredprovider.sh | sh
eot

ENV VSS_NUGET_EXTERNAL_FEED_ENDPOINTS="{\"endpointCredentials\": [{\"endpoint\":\"https://pkgs.dev.azure.com/mews/_packaging/mews/nuget/v3/index.json\", \"username\":\"docker\", \"password\":\"${FEED_ACCESSTOKEN}\"}]}"

COPY . .

RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"

RUN dotnet restore ./Mews.Job.Scheduler.sln

RUN dotnet publish "src/hosts/Mews.Job.Scheduler/Mews.Job.Scheduler.csproj" -c Release -o /app --no-restore /p:Version=$PACKAGE_VERSION

RUN if [ "${GENERATE_BUNDLE}" ]; then \
        dotnet ef migrations bundle -r linux-x64 -o "/app/migration_bundle" -p "src/modules/core/Mews.Job.Scheduler.Core.EntityFrameworkCore/Mews.Job.Scheduler.Core.EntityFrameworkCore.csproj"; \
    else \
        echo "Skipping migration bundle generation."; \
    fi

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app
COPY --from=build /app .

COPY entrypoint.sh /app/entrypoint.sh
RUN chmod +x /app/entrypoint.sh

ENTRYPOINT ["/app/entrypoint.sh"]
