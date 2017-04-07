FROM golang:1.8

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

# Install manifest-tool
RUN cd $GOPATH/src \
    && mkdir -p github.com/estesp \
    && cd github.com/estesp \
    && git clone https://github.com/estesp/manifest-tool \
    && cd manifest-tool \
    && make binary \
    && make install

# # Install powershell
# RUN apt-get update \
#     && apt-get install -y --no-install-recommends \
#         libicu55 \
#         libunwind8 \
#     && rm -rf /var/lib/apt/lists/*

# RUN curl -o powershell.deb -ssL https://github.com/PowerShell/PowerShell/releases/download/v6.0.0-alpha.15/powershell_6.0.0-alpha.15-1ubuntu1.16.04.1_amd64.deb \
#     && dpkg -i powershell.deb \
#     && rm -f powershell.deb

# WORKDIR runner
# COPY . .

ENTRYPOINT ["manifest-tool"]