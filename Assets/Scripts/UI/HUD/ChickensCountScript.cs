using System;
using Globs;
using TMPro;
using UnityEngine;

public class ChickensCountScript : MonoBehaviour
{
    public TMP_Text text;
    
    public float chickenToKrRatio = 20f;
    private float _chickenCount = 0f;

    public float Coins
    {
        get => _chickenCount * chickenToKrRatio;
        set => _chickenCount = value / chickenToKrRatio;
    }

    private void Awake()
    {
        Globals.ChickensCountScript = this;
    }

    private string Text()
    {
        return FormatMoney(Coins);
    }

    private void Start()
    {
        text.text = Text();
    }

    public void UpdateText()
    {
        text.text = Text();
    }
    
    public void IncrementChickenCount() {
        _chickenCount++;
        UpdateText();
    }
    
    public void DecrementChickenCount() {
        _chickenCount--;
        UpdateText();
    }
    
    public static string FormatMoney(float value)
    {
        string[] suffixes =
        {
            "", "thousand ", "million ", "billion ", "trillion ",
            "quadrillion ", "quintillion "
        };

        int suffixIndex = 0;

        while (value >= 1000 && suffixIndex < suffixes.Length - 1)
        {
            value /= 1000;
            suffixIndex++;
        }

        string formatted = value % 1 == 0
            ? value.ToString("0")
            : value.ToString("0.##");

        return $"{formatted} {suffixes[suffixIndex]}kr";
    }
}
