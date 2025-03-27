using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
   
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI winText;
    public TextMeshProUGUI hintText;
    public TextMeshProUGUI levelText;

    public Button shuffleBtn;
    public Button hintBtn;
    public Button backHomeBtn;

    public BoardManager boardManager;
    public CameraController cameraController;
    private GameLogic gameLogic;

    private int score = 0;
    private int level = 0;

    public float nextLevelDelayTime = 3f;
    public int pointsPerPair = 10;
    public int hintsPerLevel = 5;

    private void Awake()
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

    private void Start()
    {
        gameLogic = FindObjectOfType<GameLogic>();
        shuffleBtn.onClick.AddListener(OnShuffleButtonClicked);
        hintBtn.onClick.AddListener(OnHintButtonClicked);
        backHomeBtn.onClick.AddListener(OnBackHomeBtnClicked);

        UpdateLevelText();
        SetupBoard();
    }

    void UpdateLevelText()
    {
        if (levelText != null)
            levelText.text = $"Level {level}";
    }

    void SetupBoard()
    {
        (int rows, int cols) = GetBoardSize(level);
        boardManager.SetBoardSize(rows, cols);
        boardManager.GenerateBoard();
        cameraController.CenterCameraOnBoard();
    }

    (int, int) GetBoardSize(int level)
    {
        switch (level)
        {
            case 0:
            case 1:
                return (4, 10);
            case 2:
            case 3:
                return (6, 10);
            default:
                return (8, 15);
        }
    }

    public void AddScore()
    {
        score += pointsPerPair;
        scoreText.text = "Score: " + score;
    }

    public void ShowCongrats()
    {
        winText.text = "You win!!";
        StartCoroutine(NextLevel(nextLevelDelayTime));
    }

    private IEnumerator NextLevel(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        level++;
        UpdateLevelText();
        SetupBoard();
        ResetHintUse();
        UpdateHintUI();
        winText.text = "";
    }

    void OnShuffleButtonClicked()
    {
        if (boardManager != null)
        {
            boardManager.ShuffleTiles();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            OnHintButtonClicked();
        }
    }
    void OnHintButtonClicked()
    {
        if (gameLogic != null)
        {
            gameLogic.ShowHint();
            UpdateHintUI();
            if (gameLogic.numsOfHint <= 0)
            {
                hintBtn.image.color = Color.gray;
            }
        }
    }

    void OnBackHomeBtnClicked()
    {
        SceneManager.LoadSceneAsync(0);
    }

    public void ResetHintUse()
    {
        if (gameLogic != null)
        {
            gameLogic.numsOfHint = hintsPerLevel;
        }
        hintBtn.image.color = Color.white;
    }

    public void UpdateHintUI()
    {
        if (hintText != null && gameLogic != null)
        {
            hintText.text = $"Hint ({gameLogic.numsOfHint})";
        }
    }
}
