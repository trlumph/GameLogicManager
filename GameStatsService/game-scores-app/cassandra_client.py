from cassandra import ConsistencyLevel
from cassandra.query import SimpleStatement
from cassandra.cluster import Cluster

class CassandraClient:
    def __init__(self, hosts, port, keyspace):
        self.hosts = hosts
        self.port = port
        self.keyspace = keyspace
        self.player_scores_table = "players_scores"
        self.session = None
            
    def connect(self):
        cluster = Cluster(self.hosts, port=self.port)
        self.write_consistency = ConsistencyLevel.QUORUM
        self.read_consistency = ConsistencyLevel.ONE
        self.session = cluster.connect(self.keyspace)

    def execute(self, *args, **kwargs):
        return self.session.execute(*args, **kwargs)
    
    def execute_cons(self, query: str):
        """ Execute a query with the appropriate consistency level 
            generated based on the query text
        """
        if query.lower().startswith("select"):
            query_consistency = self.read_consistency
        else:
            query_consistency = self.write_consistency
        query_stmt = SimpleStatement(query, consistency_level=query_consistency)
        return self.execute(query_stmt)
        
    def read_execute(self, query: str):
        query_stmt = SimpleStatement(query, consistency_level=self.write_consistency)
        return self.execute(query_stmt)
    
    def write_execute(self, query: str):
        query_stmt = SimpleStatement(query, consistency_level=self.write_consistency)
        return self.execute(query_stmt)
    
    def close(self):
        self.session.shutdown()

    def __del__(self):
        if self.session is not None:
            self.close()
            
    def init_player_score(self, origin_uid:str) -> bool:
        query = (f"INSERT INTO {self.keyspace}.{self.player_scores_table}"
                 f" (player, score)"
                 f" VALUES ('{origin_uid}', 0)")
        self.execute_cons(query)
        return True
    
    def get_player_score(self, origin_uid:str) -> int|None:
        query = (f"SELECT "
                 f"score"
                 f" FROM {self.keyspace}.{self.player_scores_table}" 
                 f" WHERE player='{origin_uid}'")
        result = self.execute_cons(query).all()
        return result[0].score if result else None
    
    def update_player_score(self, origin_uid:str, score:int) -> bool:
        query = (f"UPDATE {self.keyspace}.{self.player_scores_table}"
                 f" SET score={score}"
                 f" WHERE player='{origin_uid}'")
        self.execute_cons(query)
        return True
