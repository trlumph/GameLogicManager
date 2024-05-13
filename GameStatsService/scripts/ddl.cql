CREATE KEYSPACE IF NOT EXISTS game_stats  WITH replication = {'class': 'SimpleStrategy', 'replication_factor': 1};


CREATE TABLE IF NOT EXISTS game_stats.players_scores(
    player text,
    score int,
    PRIMARY KEY (player)
);
