namespace GameService
{
    public abstract class GameRequest
    {
        public string playerId;
        public RequestType requestType;

        protected GameRequest(RequestType requestType, string playerId)
        {
            this.playerId = playerId;
            this.requestType = requestType;
        }
    }

    public class KillMonsterRequest : GameRequest
    {
        public KillMonsterRequest(RequestType requestType, string playerId) : base(requestType, playerId) { }
    }

    public class FightPlayerRequest : GameRequest
    {
        public string opponentId;

        public FightPlayerRequest(RequestType requestType, string playerId, string opponentId) : base(requestType, playerId)
        {
            this.opponentId = opponentId;
        }
    }

    public class GiftPlayerRequest : GameRequest
    {
        public string toPlayer;
        public int giftAmount;

        public GiftPlayerRequest(RequestType requestType, string playerId, string toPlayer, int giftAmount) : base(requestType, playerId)
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