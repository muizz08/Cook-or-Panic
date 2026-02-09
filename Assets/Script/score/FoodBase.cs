using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class FoodBase: MonoBehaviour
{
    public enum FoodType
    {
        Pempek,
        Onde,
    }

    public FoodType foodType;
    [SerializeField] protected int baseScore = 5;
    [SerializeField] private TMP_Text scoreText;

    [Header("Cooking State")]
    public bool isCooked;
    public bool isBurned;

    public virtual int GetScore()
    {
        if (isBurned)
            return 0;
        return baseScore;
    }

    public void ScoreFromSocket()
    {
       
        int value = GetScore();

        Debug.Log($"✅ Dapat score: {value}");
        UpdateScore(value);
    }

    public void UpdateScore(int score)
    {
        scoreText.text = score.ToString();
    }

}
