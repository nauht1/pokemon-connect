using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class GameManager : MonoBehaviour
{
    public enum GameMode { Endless, Multiplayer }
    private GameMode currentGameMode = GameMode.Endless;

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

    public string playerName { get; private set; }
    public string roomName { get; private set; }

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
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (currentGameMode == GameMode.Endless && scene.buildIndex == 3)
        {
            AssignReferencesFromScene();
            InitilizeGame();
        }

        // Connect to Photon nếu chọn Multiplayer game mode
        if (currentGameMode == GameMode.Multiplayer && !PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    void AssignReferencesFromScene()
    {
        Debug.Log("Assigned");
        SceneReferences sceneRefs = FindObjectOfType<SceneReferences>();
        if (sceneRefs == null)
        {
            Debug.LogWarning("SceneReferences not found in the scene!");
            return;
        }

        // Gán các field từ SceneReferences
        scoreText = sceneRefs.scoreText;
        winText = sceneRefs.winText;
        hintText = sceneRefs.hintText;
        levelText = sceneRefs.levelText;

        shuffleBtn = sceneRefs.shuffleBtn;
        hintBtn = sceneRefs.hintBtn;
        backHomeBtn = sceneRefs.backHomeBtn;

        boardManager = sceneRefs.boardManager;
        cameraController = sceneRefs.cameraController;
    }
    public void InitilizeGame()
    {
        gameLogic = FindObjectOfType<GameLogic>();
        if (shuffleBtn != null) 
            shuffleBtn.onClick.AddListener(OnShuffleButtonClicked);

        if (hintBtn != null)
            hintBtn.onClick.AddListener(OnHintButtonClicked);

        if (backHomeBtn != null)
            backHomeBtn.onClick.AddListener(OnBackHomeBtnClicked);

        UpdateLevelText();
        SetupBoard();
        ResetHintUse();
        UpdateHintUI();
    }

    public void SetPlayerName(string _playerName)
    {
        playerName = _playerName;
    }

    public void SetRoomName(string _roomName)
    {
        roomName = _roomName;
    }

    public void SetGameMode(GameMode _gameMode)
    {
        currentGameMode = _gameMode;
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
