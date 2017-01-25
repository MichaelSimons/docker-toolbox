# Docker 101 Lab #

This lab will not give step by step instructions to follow.  Rather it gives problems to solve using Docker and provides some guidance.  The experts in the room are here to give assistance and tips as needed.

## Install Docker ##
- Requirements (Windows 10)
	- Windows 10 Anniversary Update
	- Hyper-V enabled
	- Virtualization enabled
- `https://docs.docker.com/docker-for-windows`

## Run a Container ##
- `docker run` both a Linux and Windows container e.g. `hello-world` and `hello-world:nanoserver`
	- Hint: right click on Docker icon in the taskbar 
- `docker run -it` to run a command within a container using an interactive session
- `docker images` to find out which images exist on your machine
- `docker ps` to find out which containers exist on your machine 
	- How do you find all of them (e.g. the non-running ones)?
- `docker rm` to delete all non-running containers
- `docker rmi` to delete an image
- `docker start` to run a stopped container
- `docker attach` to a running container
- `docker exec` a command against a running container

## Create an Image ##
- Define a Dockerfile (e.g. `echo "Hello World"`)
	- Checkout `https://docs.docker.com/engine/reference/builder/`
- `docker build` your Dockerfile
- `docker run` your image

## Explore Docker Hub ##
- `http://hub.docker.com`
- Decent IDE: `https://code.visualstudio.com` + `Docker Support` extension
- Can you find `docker/whalesay`
- Create an account
- Create a repository
- `docker push` your image
	- `docker tag` it correctly first
- `docker pull` your image

## Working with data ##
- Use the `docker run -v` functionality to mount a local folder within a running container.
	- Hint: if you are running with Linux Containers on Windows you must "Share" your drive - see Docker settings.
- `docker cp` a file out of a running container

## Docker Compose ##
- Define a docker-compose.yml file that contains the configuration for a single container scenario
- `docker-compose up` your docker-compose file
- Define a docker-compose.yml file that builds an image
- `docker-compose up` your docker-compose file
- `docker-compose up` a multi-container docker-compose.yml file
	- Checkout `https://github.com/docker/example-voting-app`

## Take the Training Wheels Off ##
- Define a Nano Server based Dockerfile that downloads an artifact
	- Hint: `Powershell`'s `Invoke-WebRequest`
- Use the `SHELL` Dockerfile command to set `Powershell` as the default *shell*
- Build an application in a development image and then produce an runtime image that can be used the run the application
- What does the `HEALTHCHECK` Dockerfile command do?




 
