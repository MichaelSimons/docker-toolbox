# Use this Dockerfile to create a runner image
#     docker build -t image-builder .
#     docker run --rm --privileged -v /var/run/docker.sock:/var/run/docker.sock \
#       -v f:\repos\dotnet-docker-nightly:/repo -w /repo image-builder image-info.json

FROM microsoft/dotnet:1.1-sdk

# Install Docker
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        apt-transport-https \
        software-properties-common \
    && rm -rf /var/lib/apt/lists/*

RUN curl -fsSL https://download.docker.com/linux/debian/gpg | apt-key add - \
    && add-apt-repository "deb [arch=amd64] https://download.docker.com/linux/debian $(lsb_release -cs) stable" \
    && apt-get update \
    && apt-get install -y --no-install-recommends \
        docker-ce \
    && rm -rf /var/lib/apt/lists/*

# Build image-builder
WORKDIR image-builder
COPY *.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /out

ENTRYPOINT ["dotnet", "/out/image-builder.dll"]