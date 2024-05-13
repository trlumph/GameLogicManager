namespace GameService
{
    public abstract class GameRequest
    {
        public RequestType requestType;
        public string playerId;
        public string token;

        protected GameRequest(RequestType requestType, string playerId, string token)
        {
            this.playerId = playerId;
            this.requestType = requestType;
            this.token = token;
        }
    }

    public class KillMonsterRequest : GameRequest
    {
        public KillMonsterRequest(RequestType requestType, string playerId, string token) : base(requestType, playerId, token) { }
    }

    public class FightPlayerRequest : GameRequest
    {
        public string opponentId;

        public FightPlayerRequest(RequestType requestType, string playerId, string token, string opponentId) : base(requestType, playerId, token)
        {
            this.opponentId = opponentId;
        }
    }

    public class GiftPlayerRequest : GameRequest
    {
        public string toPlayer;
        public int giftAmount;

        public GiftPlayerRequest(RequestType requestType, string playerId, string token, string toPlayer, int giftAmount) : base(requestType, playerId, token)
        {
            this.toPlayer = toPlayer;
            this.giftAmount = giftAmount;
        }
    }

    public enum RequestType
    {
        KillMonster,
        FightPlayer,
        GiftPlayer,
    }
}