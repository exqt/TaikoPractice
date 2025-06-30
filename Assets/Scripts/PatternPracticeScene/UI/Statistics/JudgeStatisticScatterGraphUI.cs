using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class JudgeStatisticScatterGraphUI : MonoBehaviour
{
    RectTransform rectTransform;
    Texture2D texture;

    public Image pointImage;

    const int HEIGHT = 201;

    public Color pointColor = Color.green;

    int noteCount = 700;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        texture = new Texture2D((int)rectTransform.rect.width, (int)rectTransform.rect.height);

        pointImage.color = Color.white;
        pointImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

        var colors = new Color[texture.width * texture.height];
        Array.Fill(colors, Color.clear);

        texture.SetPixelData(colors, 0);
        texture.Apply();
    }

    void SetPoint(SongPlay.NoteJudge judge)
    {
        if (judge.judge == JudgeType.None) return;

        int ms = Mathf.RoundToInt((float)(judge.diff * 1000));
        int y = HEIGHT / 2 + ms;

        if (y < 0 || y >= texture.height)
        {
            Debug.LogError($"Y coordinate out of bounds: {y}. Must be between 0 and {texture.height - 1}.");
            return;
        }

        int x = (int)((float)judge.index / noteCount * (texture.width - 1));
        texture.SetPixel(x, y, pointColor);
    }

    public void SetPoints(IEnumerable<SongPlay.NoteJudge> judges)
    {
        noteCount = judges.Count();
        foreach (var judge in judges)
        {
            SetPoint(judge);
        }
        texture.Apply();
    }
}
