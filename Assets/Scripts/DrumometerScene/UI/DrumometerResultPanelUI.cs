using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DrumometerResultPanelUI : MonoBehaviour
{
    public TMP_Text title;
    public TMP_Text score;
    public TMP_Text maxBPMText;
    public Image bpmGraphImage;

    Color graphColor = new(0.2f, 0.7f, 1f, 1f);

    public void SetData(string titleText, string scoreText, string maxBPMText)
    {
        this.maxBPMText.text = maxBPMText;
        title.text = titleText;
        score.text = scoreText;
    }

    public void OnGoToMain()
    {
        SceneUtil.BackToMainScene();
    }

    public void OnRetry()
    {
        SceneUtil.ReloadCurrentScene();
    }

    public void SetBPMGraph(float[] bpmArray)
    {
        if (bpmGraphImage == null) return;
        int width = bpmGraphImage.rectTransform.rect.width > 0 ? (int)bpmGraphImage.rectTransform.rect.width : 300;
        int height = bpmGraphImage.rectTransform.rect.height > 0 ? (int)bpmGraphImage.rectTransform.rect.height : 100;
        var tex = new Texture2D(width, height);
        tex.SetPixels(new Color[width * height]);
        int n = bpmArray.Length;
        float maxBpm = 0;
        for (int i = 0; i < n; ++i) if (bpmArray[i] > maxBpm) maxBpm = bpmArray[i];
        if (maxBpm < 1) maxBpm = 1;
        int prevX = 0, prevY = 0;
        for (int i = 0; i < n; ++i)
        {
            int x = (int)(i * (width - 1) / (float)(n - 1));
            int y = (int)(bpmArray[i] / maxBpm * (height - 1));
            if (i > 0)
            {
                DrawLine(tex, prevX, prevY, x, y, graphColor);
            }
            prevX = x;
            prevY = y;
        }
        tex.Apply();
        bpmGraphImage.sprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
        bpmGraphImage.color = Color.white; // Reset color to white to show the graph
    }

    // Bresenham's line algorithm for drawing lines on Texture2D
    void DrawLine(Texture2D tex, int x0, int y0, int x1, int y1, Color color)
    {
        int dx = Mathf.Abs(x1 - x0), dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;
        while (true)
        {
            if (x0 >= 0 && x0 < tex.width && y0 >= 0 && y0 < tex.height)
                tex.SetPixel(x0, y0, color);
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
    }
}
