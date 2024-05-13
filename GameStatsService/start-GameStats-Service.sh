docker compose -f docker-compose.yml up cassandra-0 cassandra-1 -d --wait

docker compose -f docker-compose.yml up game-scrores -d --wait

sh scripts/build-app-image.sh

sh scripts/run-game-scores-rest-.sh
