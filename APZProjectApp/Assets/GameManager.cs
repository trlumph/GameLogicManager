using System;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private AuthManager authManager;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_InputField playerToFightInput;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void EnableGame()
    {
        gameObject.SetActive(true);

        UpdateScore();
    }
    public void DisableGame()
    {
        gameObject.SetActive(false);
    }

    public async void KillEnemy()
    {
        var req = await authManager.Client.PostAsync($"http://localhost:5074/killMonster?playerId={authManager.CurrentUsername}&token={authManager.AuthToken}", null);
        messageText.SetText(req.IsSuccessStatusCode ? "Killed enemy" : "Failed to kill enemy");

        UpdateScore();
    }

    public async void FightPlayer()
    {
        var req = await authManager.Client.PostAsync($"http://localhost:5074/fightPlayer?playerId={authManager.CurrentUsername}&token={authManager.AuthToken}&opponentId={playerToFightInput.text}", null);
        messageText.SetText(req.IsSuccessStatusCode ? "Fought player" : "Failed to fight player");

        UpdateScore();
    }

    public async void UpdateScore()
    {
        var response = await authManager.Client.GetAsync($"http://localhost:8181/scores/user/{authManager.CurrentUsername}");
        scoreText.SetText(response.IsSuccessStatusCode ? response.Content.ReadAsStringAsync().Result : response.StatusCode.ToString());
    }

    [Serializable]
    private class ScoreResponse
    {
        public string score;
    }
}