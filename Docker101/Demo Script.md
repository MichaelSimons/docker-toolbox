# Demo Scenarios #

## CLI Basics ##

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
12. `docker rm –f whalesay`
13. `docker rmi docker/whalesay`


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

## Layers ##
1. `https://microbadger.com/images/microsoft/dotnet-nightly`


## Dockerfile ##

Switch to Linux Containers

1. Walkthrough and discuss 'https://github.com/dotnet/dotnet-docker-samples/blob/master/dotnetapp-dev'
	1. Talk about various commands
	2. Mention layers concept
2. docker build -t dotnetbot .
3. docker run –it –rm dotnetbot
4. Edit default message
5. docker build -t dotnetbot .
	1. Note layers usage when building



