
# Game Stats (Scores) API

## How to run the project

```bash
docker-compose up --wait
```

or
  
```bash
sh run-GamesStats-service.sh
```

## Endpoints

- `POST: /scores/user/{userId}` - Create a new user stats instance

- `POST: /scores/user/{userId}/add?score={score}` - Add a score to a user

- `GET: /scores/user/{userId}` - Get a user stats

- `POST: /scores/user/{userId}/clear` - Clear all user stats to 0

- `GET: /` and `GET: /health` - health check



