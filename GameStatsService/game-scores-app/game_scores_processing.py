import cassandra_client
import os

def get_cassandra_connection():
    """ Get a connection to the cassandra cluster """
    port = os.getenv('CASSANDRA_PORT', 9042)
    host = os.getenv('CASSANDRA_HOSTS', 'cassandra-0,cassandra-1,cassandra-2')
    keyspace = os.getenv('CASSANDRA_KEYSPACE', 'game_stats')
    hosts = host.split(',')
    hosts = ['cassandra-0']
    db_connection = cassandra_client.CassandraClient(hosts, port, keyspace)
    db_connection.connect()
    return db_connection

class GameScoresService:
    def __init__(self, db_connection=None):
        if db_connection is None:
            db_connection = get_cassandra_connection()
        self.db_connection = db_connection
        
    def player_exists(self, player_name: str) -> bool:
        """Check if a player exists in the database"""
        result = self.db_connection.get_player_score(player_name)
        return False if result is None else True

    def find_player_scores(self, player_name: str) -> int|None:
        """Find the scores for a player in the database"""
        result = self.db_connection.get_player_score(player_name)
        return result

    def add_player_score(self, player_name: str, score: int) -> int|None:
        """Add a score to a player in the database"""
        if not self.player_exists(player_name):
            return None
        prev_score = self.db_connection.get_player_score(player_name)
        new_score = prev_score + score
        self.db_connection.update_player_score(player_name, new_score)
        return new_score

    def add_player(self, player_name: str) -> bool|None:
        """ Add a player to the database"""
        if self.player_exists(player_name):
            return None
        self.db_connection.init_player_score(player_name)
        return True

    def clear_player_score(self, player_name: str) -> bool|None:
        """Clear a player's score in the database"""
        if not self.player_exists(player_name):
            return None
        self.db_connection.update_player_score(player_name, 0)
        return True
