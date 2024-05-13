using System.Net.Http;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class AuthManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button logoutButton;
    [SerializeField] private GameManager gameManager;

    public HttpClient Client { get; } = new();

    public string CurrentUsername { get; private set; }
    public string AuthToken { get; private set; }

    public async void Register()
    {
        var username = usernameInput.text;
        var password = passwordInput.text;

        var form = new WWWForm();
        form.AddField("name", username);
        form.AddField("password", password);

        var jsonBody = "{\"name\":\"" + username + "\",\"password\":\"" + password + "\"}";
        var request = await Client.PostAsync($"http://localhost:5064/register", new StringContent(jsonBody, Encoding.UTF8, "application/json"));

        if (request.IsSuccessStatusCode)
        {
            CurrentUsername = username;
            messageText.SetText("Registered successfully");
        }
        else
            messageText.SetText("Failed to register");
    }
    public async void Login()
    {
        var username = usernameInput.text;
        var password = passwordInput.text;

        var form = new WWWForm();
        form.AddField("name", username);
        form.AddField("password", password);

        var jsonBody = "{\"name\":\"" + username + "\",\"password\":\"" + password + "\"}";
        var request = await Client.PostAsync($"http://localhost:5064/login", new StringContent(jsonBody, Encoding.UTF8, "application/json"));

        if (request.IsSuccessStatusCode)
        {
            CurrentUsername = username;
            AuthToken = JsonUtility.FromJson<AuthResponse>(request.Content.ReadAsStringAsync().Result).token;

            messageText.SetText("Logged in successfully");

            logoutButton.gameObject.SetActive(true);
            gameManager.EnableGame();
        }
        else
            messageText.SetText("Failed to log in");
    }

    public async void Logout()
    {
        var jsonBody = "{\"name\":\"" + CurrentUsername + "\",\"token\":\"" + AuthToken + "\"}";
        var request = await Client.PostAsync($"http://localhost:5064/logout", new StringContent(jsonBody, Encoding.UTF8, "application/json"));

        if (request.IsSuccessStatusCode)
        {
            CurrentUsername = null;
            AuthToken = null;
            messageText.SetText("Logged out successfully");

            logoutButton.gameObject.SetActive(false);
            gameManager.DisableGame();
        }
        else
            messageText.SetText("Failed to log out");
    }

    [System.Serializable]
    private record AuthResponse
    {
        public string token;
    }
}