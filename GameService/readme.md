# Game API

## Endpoints

- `POST: /killMonster?playerId={playerId}&token={token}` - Kill monster by player, token is for validation

- `POST: /fightPlayer?playerId={playerId}&token={token}` - Fight another player. The player with higher score gets all points from the other player.

- `POST: /giftPlayer?playerId={playerId}&token={token}&toPlayer={toPlayer}&giftAmount={giftAmount}` - Gift another player a certain amount of points.