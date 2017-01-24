# Demo Scenarios #

## CLI Demo ##

Switch to Linux Containers

1. `docker run docker/whalesay cowsay Hello Fargo`
2. `docker run docker/whalesay cowsay Hello Developers`
3. `docker run –it –name whalesay docker/whalesay`
4. `cowsay It is cold here!`
5. `ls`
6. `exit`
7. `docker images`
8. `docker ps -a`
9. `docker start whalesay`
10. `docker ps`
11. `docker exec want to go swimming?`
12. `docker exec how about fishing instead?`
13. `docker rm –f whalesay`
14. `docker rmi docker/whalesay`


Switch to Windows Containers

1. `docker images`
2. `docker pull microsoft/dotnet-samples:dotnetapp-nanoserver`
3. `docker images`
4. `docker tag microsoft/dotnet-samples:dotnetapp-nanoserver dotnetapp`
5. `docker images`
6. `docker run –rm dotnetapp Where did that whale go?`
7. `docker ps –a`
8. `docker system df`
9. `docker system prune`
10. `docker ps -a`

## Dockerfile Demo ##

Switch to Linux Containers

1. `https://github.com/dotnet/dotnet-docker-samples/blob/master/dotnetapp-dev`
2. `docker build -t dotnetbot .`
3. `docker run –it –rm dotnetbot`
4. Edit default message
5. `docker build -t dotnetbot .`
6. `https://microbadger.com/images/msimons/docker101`


## Mounted Volume Demo ##
1.  `docker run -it --rm -v c:\repos\docker-toolbox\docker101\dotnetbot:/samples/ microsoft/dotnet:1.1-sdk-msbuild`
2.  cd dotnetapp-dev
3.  cat Program.cs
4.  modify locally

## Docker Hub Demo ##
1. `https://hub.docker.com`
2. Official Repositories
3. Tags and the scheme followed
4. Private Repository
5. autobuild


## Compose Demo ##
Simple demo to store configuration

Cats vs dogs demo
1. `https://github.com/docker/example-voting-app`
2. `docker-compose up`
3. `http://localhost:5000`
4. `http://localhost:5001`

## Visual Studio Demo ##
1.  Create a web app with Docker
2.  Build in background
3.  Show Compose
3.  F5
4.  Debug
5.  Modify source
