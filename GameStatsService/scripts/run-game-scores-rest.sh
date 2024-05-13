#!/bin/bash

CMD="uvicorn rest_application:app --host 0.0.0.0 --port 8080 --reload"

if [ -z "$command" ]; then
    command=$CMD
fi

network="game-logic-network"

app_directory="game-scores-app"
app_image=$app_directory
app_container=$app_image-server

docker rm -f $app_container >/dev/null 2>&1 || true

docker run --rm \
        --network $network \
        --name $app_container \
        -v $(pwd)/$app_directory:/app \
        -p 8080:8080 \
        -it $app_image \
        $command
    
echo "$app_container container exited"
