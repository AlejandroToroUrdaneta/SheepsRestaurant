using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HUD : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;

    float elapsedTime;

    public static int Score;

    private void Start()
    {
        Score = 0;
    }

    private void Update()
    {
        scoreText.text = Score.ToString();
        elapsedTime += Time.deltaTime;
        int minutes = Mathf.FloorToInt(elapsedTime / 60);
        int seconds = Mathf.FloorToInt(elapsedTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}",minutes,seconds);

        if(Score >= 100)
        {
            GameEnding.result = "win";
            GameEnding.End = true;
        }
    }

    public void ChangeUIScore()
    {
        
    }
}
