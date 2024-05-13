docker rm -f game-scores-app-server >/dev/null 2>&1 || true

docker run --rm --network game-logic-network --name game-scores-app-server -p 8080:8080 -it game-scores-app  "uvicorn rest_application:app --host 0.0.0.0 --port 8080 --reload"
		