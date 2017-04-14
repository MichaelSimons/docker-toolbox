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

ENTRYPOINT ["manifest-tool"]