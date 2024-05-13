using GameService;

namespace MessagesService;

public class GameManager
{
    private const int EnemyKillReward = 100;
    private const string ScoreServiceUrl = "http://localhost:8181";
    private const string AuthServiceUrl = "http://localhost:5064";

    private HttpClient _client = new();
    private Database _database = null!;

    private readonly Queue<GameRequest> _requests = new();

    public void SetDatabase(Database database)
    {
        _database = database;
    }

    public void PostKillMonsterRequest(string playerId, string token)
    {
        _requests.Enqueue(new KillMonsterRequest(RequestType.KillMonster, playerId, token));
    }
    public void PostFightPlayerRequest(string playerId, string token, string opponentId)
    {
        _requests.Enqueue(new FightPlayerRequest(RequestType.FightPlayer, playerId, token, opponentId));
    }
    public void PostGiftPlayerRequest(string playerId, string token, string toPlayer, int giftAmount)
    {
        _requests.Enqueue(new GiftPlayerRequest(RequestType.GiftPlayer, playerId, token, toPlayer, giftAmount));
    }

    public async Task HandleRequestsLoop(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (_requests.Count <= 0)
            {
                await Task.Delay(50, cancellationToken);
                continue;
            }

            var request = _requests.Dequeue();
            var isValid = await _client.GetAsync($"{AuthServiceUrl}/validate?name={request.playerId}&token={request.token}", cancellationToken);
            if (!isValid.IsSuccessStatusCode)
            {
                Console.WriteLine($"Invalid request from player {request.playerId}.");
                continue;
            }

            await _database.CreateAsync(new Database.ActionLog
            {
                ActionType = request.GetType().ToString(),
                PlayerId = request.playerId
            });

            switch (request)
            {
                case KillMonsterRequest killMonsterRequest:
                    var requestUri = $"{ScoreServiceUrl}/scores/user/{killMonsterRequest.playerId}/add?score={EnemyKillReward}";
                    await _client.PostAsync(requestUri, new StringContent(""), cancellationToken);

                    Console.WriteLine($"Killing monster for player {killMonsterRequest.playerId}.");
                    break;
                case FightPlayerRequest fightPlayerRequest:
                    var p1Score = await GetPlayerScore(fightPlayerRequest.playerId);
                    var p2Score = await GetPlayerScore(fightPlayerRequest.opponentId);

                    var winner = p1Score > p2Score ? fightPlayerRequest.playerId : fightPlayerRequest.opponentId;
                    var loser = p1Score > p2Score ? fightPlayerRequest.opponentId : fightPlayerRequest.playerId;
                    var scoreToGive = p1Score > p2Score ? p2Score : p1Score;

                    var clearScoreUri = $"{ScoreServiceUrl}/scores/user/{loser}/clear";
                    await _client.PostAsync(clearScoreUri, new StringContent(""), cancellationToken);

                    var giveScoreUri = $"{ScoreServiceUrl}/scores/user/{winner}/add?score={scoreToGive}";
                    await _client.PostAsync(giveScoreUri, new StringContent(""), cancellationToken);
                    break;
                case GiftPlayerRequest giftPlayerRequest:
                    Console.WriteLine($"Gifting {giftPlayerRequest.giftAmount} to player {giftPlayerRequest.toPlayer} from player {giftPlayerRequest.playerId}.");
                    break;
            }
        }
    }

    private async Task<int> GetPlayerScore(string playerId)
    {
        var getRequestUri = $"{ScoreServiceUrl}/scores/user/{playerId}";
        var response = await _client.GetAsync(getRequestUri);
        if (!response.IsSuccessStatusCode) return 0;
        var score = int.Parse(await response.Content.ReadAsStringAsync());
        return score;
    }

    public async Task<List<Database.ActionLog>> GetLogs()
    {
        return await _database.GetAsync();
    }
}