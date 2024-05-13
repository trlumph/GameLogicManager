#!/bin/bash

app_image=game-scores-app
app_directory=./$app_image

# delete the previous image if it exists
docker rmi $app_image:latest --force

docker build -t $app_image $app_directory
