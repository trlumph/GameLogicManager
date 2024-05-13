from fastapi import FastAPI, HTTPException, Path, Query
from game_scores_processing import GameScoresService
import logging

# Initialize logging
logging.basicConfig(level=logging.INFO,
                    format='|%(asctime)s| - |%(name)s| - |%(levelname)s| - |%(message)s|')
logger = logging.getLogger(__name__)

app = FastAPI()
game_scores_service = GameScoresService()

@app.get("/scores/user/{player_name}")
def get_player_scores(player_name: str = Path(..., title="The name of the player whose scores you want to retrieve")):
    logger.info(f"Retrieving score for player {player_name}")
    try:
        result = game_scores_service.find_player_scores(player_name)
    except Exception as e:
        logger.error(f"Error from db {player_name}: {e}")
        raise HTTPException(status_code=500, detail="Error retrieving player score")
    if result is None:
        logger.error(f"Player {player_name} not found")
        raise HTTPException(status_code=404, detail="Player not found")
    logger.info(f"Successfully retrieved score for player {player_name}")
    return result
    # return {"player_name": player_name, "score": result, "status": 200}

@app.post("/scores/user/{player_name}")
def add_player(player_name: str = Path(..., title="The name of the player you want to add")):
    logger.info(f"Adding player {player_name}")
    try:
        result = game_scores_service.add_player(player_name)
    except Exception as e:
        logger.error(f"Error from db {player_name}: {e}")
        raise HTTPException(status_code=500, detail="Error adding player")
    if result is None:
        logger.error(f"Player {player_name} already exists")
        raise HTTPException(status_code=400, detail="Player already exists")
    logger.info(f"Successfully added player {player_name}")
    return {"message": "Player added successfully", "player_name": player_name, "status": 200}

@app.post("/scores/user/{player_name}/add")
def add_player_score(player_name: str = Path(..., title="The name of the player whose scores you want to update"),
                           score: int = Query(..., title="The score to add to the player's current score")):
    logger.info(f"Adding score {score} to player {player_name}")
    try:
        result = game_scores_service.add_player_score(player_name, score)
    except Exception as e:
        logger.error(f"Error from db {player_name}: {e}")
        raise HTTPException(status_code=500, detail="Error adding player score")
    if result is None:
        logger.error(f"Player {player_name} not found")
        raise HTTPException(status_code=404, detail="Player not found")
    logger.info(f"Successfully added score {score} to player {player_name}")
    return {"message": "Score added successfully", "player_name": player_name, "new_score": result, "status": 200}

@app.post("/scores/user/{player_name}/clear")
def clear_player_score(player_name: str = Path(..., title="The name of the player whose scores you want to clear")):
    logger.info(f"Clearing scores for player {player_name}")
    try:
        result = game_scores_service.clear_player_score(player_name)
    except Exception as e:
        logger.error(f"Error from db {player_name}: {e}")
        raise HTTPException(status_code=500, detail="Error clearing player scores")
    if result is None:
        logger.error(f"Player {player_name} not found")
        raise HTTPException(status_code=404, detail="Player not found")
    logger.info(f"Successfully cleared scores for player {player_name}")
    return {"message": "Score cleared successfully", "player_name": player_name, "status": 200}

@app.get("/health")
@app.get("/")
def read_root():
    return {"message": "Welcome to the Scores Service API!"}
