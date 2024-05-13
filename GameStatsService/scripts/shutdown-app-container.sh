#!/bin/bash

container_name=$1
if [ -z "$container_name" ]; then
    container_name="rest"
fi
container_name="$container_name-app-server"

docker stop $container_name >/dev/null 2>&1 || true
docker remove $container_name >/dev/null 2>&1 || true
