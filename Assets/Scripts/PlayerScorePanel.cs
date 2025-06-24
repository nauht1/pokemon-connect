using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerScorePanel : MonoBehaviour
{
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI scoreText;

    public void SetPlayerInfo(string playerName, int score)
    {
        playerNameText.text = playerName;
        scoreText.text = "Score: " + score;
    }
}
