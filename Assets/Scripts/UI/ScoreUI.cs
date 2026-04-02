using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] private TMP_Text score;
    [SerializeField] private GameObject[] scoreStates;

    private int scoreValue = 50;

    private void Update()
    {
        if (scoreValue <= 33)
        {
            scoreStates[0].active = true;
            scoreStates[1].active = false;
            scoreStates[2].active = false;
        }

        if (scoreValue > 33 && scoreValue <= 66)
        {
            scoreStates[0].active = false;
            scoreStates[1].active = true;
            scoreStates[2].active = false;
        }

        if (scoreValue > 66)
        {
            scoreStates[0].active = false;
            scoreStates[1].active = false;
            scoreStates[2].active = true;
        }

        score.text = scoreValue.ToString();
    }

    public void AddScore(int score)
    {
        scoreValue += score;
    }
}
