using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SceneReferences : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI winText;
    public TextMeshProUGUI hintText;
    public TextMeshProUGUI levelText;

    public Button shuffleBtn;
    public Button hintBtn;
    public Button backHomeBtn;

    public BoardManager boardManager;
    public CameraController cameraController;

    public PlayerScorePanel[] playerScorePanels;

    public static SceneReferences Instance {get; private set;}

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
}
