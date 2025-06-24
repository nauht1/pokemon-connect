using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Unity.VisualScripting;

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

    private Dictionary<int, int> playerScores = new Dictionary<int, int>();
    private Dictionary<int, string> playerNames = new Dictionary<int, string>();

    public PlayerScorePanel[] playerScorePanels;

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
        AssignReferencesFromScene();

        if (currentGameMode == GameMode.Endless && scene.buildIndex == 3)
        {
            InitilizeGame();
        }

        // Connect to Photon nếu chọn Multiplayer game mode
        if (currentGameMode == GameMode.Multiplayer && !PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }  

        if (currentGameMode == GameMode.Multiplayer && PhotonNetwork.IsConnected && scene.buildIndex == 4)
        {
            InitilizeGame();
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public GameMode GetCurrentGameMode()
    {
        return currentGameMode;
    }

    void AssignReferencesFromScene()
    {
        Debug.Log("ASsign");
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
        playerScorePanels = sceneRefs.playerScorePanels;
    }

    public void SetPlayerName(string _playerName)
    {
        playerName = _playerName;
        if (currentGameMode == GameMode.Multiplayer && PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            Debug.Log("Set player name");
            PhotonNetwork.LocalPlayer.NickName = _playerName;
            NetworkManager.Instance.SyncPlayerName(PhotonNetwork.LocalPlayer.ActorNumber, _playerName);
        }
    }

    public void OnPlayerNameSynced(int actorNumber, string name)
    {
        playerNames[actorNumber] = name;
        UpdateScoreUI();
    }

    public void AddScore(int playerActorNumber)
    {
        if (!playerScores.ContainsKey(playerActorNumber))
        {
            playerScores[playerActorNumber] = 0;
        }

        playerScores[playerActorNumber] += pointsPerPair;
        UpdateScoreUI();

        if (currentGameMode == GameMode.Multiplayer)
        {
            NetworkManager.Instance.UpdateScore(playerActorNumber, playerScores[playerActorNumber]);
        }
    }

    public void OnScoreUpdated(int playerActorNumber, int newScore)
    {
        playerScores[playerActorNumber] = newScore;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (currentGameMode == GameMode.Endless)
        {
            if (scoreText != null)
            {
                int score = playerScores.ContainsKey(0) ? playerScores[0] : 0;
                scoreText.text = "Score: " + score;
            }
        }
        else if (currentGameMode == GameMode.Multiplayer)
        {
            Debug.Log(playerScorePanels.Length);
            for (int i = 0; i < playerScorePanels.Length; i++)
            {
                if (i < PhotonNetwork.PlayerList.Length)
                {
                    int actorNumber = PhotonNetwork.PlayerList[i].ActorNumber;
                    string playerName = playerNames.ContainsKey(actorNumber) ? playerNames[actorNumber] : "Unknown";

                    // Check if the player has a score before trying to access it
                    int score = playerScores.ContainsKey(actorNumber) ? playerScores[actorNumber] : 0;
    
                    Debug.Log($"Updating UI for player {actorNumber}: Name = {playerName}, Score = {score}");
                    playerScorePanels[i].gameObject.SetActive(true);
                    playerScorePanels[i].SetPlayerInfo(playerName, score);
                }
                else
                {
                    playerScorePanels[i].gameObject.SetActive(false);
                }
            }
        }
    }

    public void InitilizeGame()
    {
        if (currentGameMode == GameMode.Multiplayer)
        {
            Debug.Log("Delay 1s");
            StartCoroutine(DelayInit(0.5f));
            // Initialize scores for all players
            InitializePlayerScores();
            if (!PhotonNetwork.IsMasterClient)
            {
                return; // Client sẽ chờ RPC từ Master
            }
        }

        gameLogic = FindObjectOfType<GameLogic>();
        if (shuffleBtn != null) 
            shuffleBtn.onClick.AddListener(OnShuffleButtonClicked);

        if (hintBtn != null)
            hintBtn.onClick.AddListener(OnHintButtonClicked);

        if (backHomeBtn != null)
            backHomeBtn.onClick.AddListener(OnBackHomeBtnClicked);

        UpdateLevelText();
        StartCoroutine(SetupBoard());

        if (currentGameMode == GameMode.Endless)
            ResetHintUse();

        UpdateHintUI();
    }

    private IEnumerator DelayInit(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
    }

    //public void SetPlayerName(string _playerName)
    //{
    //    playerName = _playerName;
    //}

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

    IEnumerator SetupBoard()
    {
        if (boardManager == null)
        {
            yield break;
        }

        if (cameraController == null)
        {
            yield break;

        }

        if (currentGameMode == GameMode.Endless)
        {
            (int rows, int cols) = GetBoardSize(level);
            boardManager.SetBoardSize(rows, cols);
            boardManager.GenerateBoard();
        }
        else if (currentGameMode == GameMode.Multiplayer)
        {
            // Tạo board nếu là Host
            if (PhotonNetwork.IsMasterClient)
            {
                boardManager.SetBoardSize(10, 16);
                boardManager.GenerateBoard();
                boardManager.SendBoardToClients();
            }
        }

        yield return new WaitForSeconds(0.5f);

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

    private void InitializePlayerScores()
    {
        Debug.Log("Initializing player scores for all players in room");

        // Clear existing scores
        playerScores.Clear();

        // Initialize scores for all players currently in the room
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (!playerScores.ContainsKey(player.ActorNumber))
            {
                playerScores[player.ActorNumber] = 0;
                Debug.Log($"Initialized score for player {player.ActorNumber}");
            }
        }

        UpdateScoreUI();
    }

    public void AddScoreEndless()
    {
        score += pointsPerPair;
        scoreText.text = "Score: " + score;
    }

    public void ShowCongrats()
    {
        winText.text = "You win!!";
        if (currentGameMode == GameMode.Endless)
        {
            StartCoroutine(NextLevel(nextLevelDelayTime));
        } 
        else if (currentGameMode == GameMode.Multiplayer)
        {
            StartCoroutine(ReturnToLobby(nextLevelDelayTime));
        }
    }

    private IEnumerator NextLevel(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        level++;
        UpdateLevelText();
        StartCoroutine(SetupBoard());
        ResetHintUse();
        UpdateHintUI();
        winText.text = "";
    }

    private IEnumerator ReturnToLobby(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        SceneManager.LoadSceneAsync(2);
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
