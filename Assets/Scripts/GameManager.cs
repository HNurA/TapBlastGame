using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    public TextMeshProUGUI scoreText;
    public Button restartButton;
    public BoardManager boardManager;
    
    private int currentScore = 0;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
        
        UpdateScoreDisplay();
    }

    public void AddScore(int blockCount)
    {
        currentScore += blockCount * 100;
        UpdateScoreDisplay();
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore}";
        }
    }

    public void RestartGame()
    {
        currentScore = 0;
        UpdateScoreDisplay();
        if (boardManager != null)
        {
            boardManager.RestartGame();
        }
    }
}