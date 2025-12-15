using UnityEngine;
using TMPro;
using Dan.Main;

public class LeaderboardManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text[] _namesTextObjects;   // Name column
    [SerializeField] private TMP_Text[] _scoresTextObjects;  // Score column
    [SerializeField] private TMP_InputField _usernameInputField;
    [SerializeField] private TMP_Text _currentScoreText;

    private string PlayerName
    {
        get => PlayerPrefs.GetString("PlayerName", "");
        set => PlayerPrefs.SetString("PlayerName", value);
    }

    public class TestEntry
    {
        public string Username;
        public int Score;

        public TestEntry(string username, int score)
        {
            Username = username;
            Score = score;
        }
    }

    private string[] funnyNames = new string[]
    {
        "PikachuFan69", "BaconOverlord", "ToiletWizard", "ChickenNugget", "BananaBandit",
        "FartDestroyer", "SpaghettiNinja", "MemeLord420", "DuckKing", "SausageMonster"
    };

    private void Start()
    {
        if (!string.IsNullOrEmpty(PlayerName))
            _usernameInputField.text = PlayerName;

        RefreshCurrentScore();
        LoadEntries();
    }

    private void Update()
    {
        RefreshCurrentScore();
    }

    private void RefreshCurrentScore()
    {
        if (_currentScoreText != null)
            _currentScoreText.text = $"Your Score: {Score.score}";
    }

    public void SaveNameAndUploadScore()
    {
        string name = _usernameInputField.text;
        if (string.IsNullOrEmpty(name)) return;

        PlayerName = name;
        StartCoroutine(UploadNextFrame());
    }

    private System.Collections.IEnumerator UploadNextFrame()
    {
        yield return null;
        int playerScore = Score.score;

        Leaderboards.FlappyBreadLeaderBoard.UploadNewEntry(PlayerName, playerScore, success =>
        {
            if (success)
                LoadEntries();
        });
    }

    public void LoadEntries()
    {
        Leaderboards.FlappyBreadLeaderBoard.GetEntries(entries =>
        {
            int total = Mathf.Max(entries.Length, Mathf.Max(_namesTextObjects.Length, _scoresTextObjects.Length));
            var fullEntries = new TestEntry[total];

            // Add current leaderboard entries
            int count = 0;
            foreach (var e in entries)
            {
                fullEntries[count++] = new TestEntry(e.Username, e.Score);
            }

            // Add test/funny entries if needed
            for (int i = count; i < total; i++)
            {
                string name = funnyNames[i % funnyNames.Length];
                fullEntries[i] = new TestEntry(name, Random.Range(1, 10));
            }

            // Place the current player's score in the correct position
            bool playerAdded = false;
            for (int i = 0; i < fullEntries.Length; i++)
            {
                if (!playerAdded && PlayerName == _usernameInputField.text && Score.score <= fullEntries[i].Score)
                {
                    fullEntries[i] = new TestEntry(PlayerName, Score.score);
                    playerAdded = true;
                }
            }

            // Sort scores from lowest to highest
            System.Array.Sort(fullEntries, (a, b) => a.Score.CompareTo(b.Score));

            // Safely fill TMP_Text columns
            int safeLength = Mathf.Min(_namesTextObjects.Length, _scoresTextObjects.Length);

            for (int i = 0; i < safeLength; i++)
            {
                if (i < fullEntries.Length)
                {
                    _namesTextObjects[i].text = fullEntries[i].Username;
                    _scoresTextObjects[i].text = fullEntries[i].Score.ToString();
                }
                else
                {
                    _namesTextObjects[i].text = "";
                    _scoresTextObjects[i].text = "";
                }
            }
        });
    }
}
