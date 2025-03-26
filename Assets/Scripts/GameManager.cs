using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
   
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI winText;
    public TextMeshProUGUI hintText;

    public Button shuffleBtn;
    public Button hintBtn;

    public BoardManager boardManager;
    private GameLogic gameLogic;

    private int score = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        gameLogic = FindObjectOfType<GameLogic>();
        shuffleBtn.onClick.AddListener(OnShuffleButtonClicked);
        hintBtn.onClick.AddListener(OnHintButtonClicked);
    }

    public void AddScore(int points)
    {
        score += points;
        scoreText.text = "Score: " + score;
    }

    public void ShowCongrats()
    {
        winText.text = "You win!!";
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
            hintText.text = $"Hint ({gameLogic.numsOfHint.ToString()})";
            if (gameLogic.numsOfHint <= 0)
            {
                hintBtn.image.color = Color.gray;
            }
        }
    }
}
