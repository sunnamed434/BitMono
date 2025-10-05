using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Game Manager script for testing BitMono obfuscation
/// Contains various patterns that should be obfuscated
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField]
    private string gameName = "BitMono Test Game";
    
    [SerializeField]
    private int maxScore = 1000;
    
    [SerializeField]
    private float gameSpeed = 1.0f;
    
    private int currentScore = 0;
    private bool isGameRunning = false;
    private string playerName = "TestPlayer";
    private float gameTime = 0f;
    
    private static GameManager instance;
    private static string staticSecret = "Static secret value";
    
    public int CurrentScore => currentScore;
    public bool IsGameRunning => isGameRunning;
    public string PlayerName => playerName;
    
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
#if UNITY_2020_1_OR_NEWER
                instance = FindFirstObjectByType<GameManager>();
#else
                instance = FindObjectOfType<GameManager>();
#endif
            }
            return instance;
        }
    }
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        Debug.Log("GameManager started");
        InitializeGame();
    }
    
    void Update()
    {
        if (isGameRunning)
        {
            UpdateGame();
        }
        
        if (Input.GetKeyDown(KeyCode.G))
        {
            StartGame();
        }
        
        if (Input.GetKeyDown(KeyCode.H))
        {
            EndGame();
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }
    
    private void InitializeGame()
    {
        Debug.Log($"Initializing game: {gameName}");
        currentScore = 0;
        gameTime = 0f;
        isGameRunning = false;
        
        LoadPlayerData();
        SetupGameSettings();
    }
    
    private void LoadPlayerData()
    {
        Debug.Log($"Loading player data for: {playerName}");
        currentScore = Random.Range(0, 100);
    }
    
    private void SetupGameSettings()
    {
        Debug.Log($"Setting up game with speed: {gameSpeed}");
        Time.timeScale = gameSpeed;
    }
    
    private void UpdateGame()
    {
        gameTime += Time.deltaTime;
        
        if (gameTime > 1f)
        {
            AddScore(10);
            gameTime = 0f;
        }
    }
    
    private void AddScore(int points)
    {
        currentScore += points;
        Debug.Log($"Score added: {points}, Total: {currentScore}");
        
        if (currentScore >= maxScore)
        {
            WinGame();
        }
    }
    
    private void WinGame()
    {
        Debug.Log("Game won!");
        EndGame();
    }
    
    public void StartGame()
    {
        Debug.Log("Starting game");
        isGameRunning = true;
        gameTime = 0f;
    }
    
    public void EndGame()
    {
        Debug.Log("Ending game");
        isGameRunning = false;
        SaveGameData();
    }
    
    public void RestartGame()
    {
        Debug.Log("Restarting game");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void SaveGameData()
    {
        Debug.Log($"Saving game data for {playerName}");
        Debug.Log($"Final score: {currentScore}");
    }
    
    private bool ValidateGameState()
    {
        if (currentScore < 0)
            return false;
            
        if (gameTime < 0)
            return false;
            
        if (string.IsNullOrEmpty(playerName))
            return false;
            
        return true;
    }
    
    private string GenerateSecretCode()
    {
        string code = "";
        code += playerName.Substring(0, Mathf.Min(3, playerName.Length));
        code += currentScore.ToString("000");
        code += gameTime.ToString("F2").Replace(".", "");
        return code;
    }
    
    public static void TestStaticMethod()
    {
        Debug.Log($"Static method called: {staticSecret}");
    }
}