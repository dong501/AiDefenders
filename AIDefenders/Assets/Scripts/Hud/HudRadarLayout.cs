using System.Collections.Generic;
using UnityEngine;

public static class HudRadarLayout
{
    public static Rect Resolve(Rect canvas, float requestedSize, float margin, IReadOnlyList<Rect> obstacles)
    {
        const float gap = 18f;
        float size = Mathf.Min(requestedSize, canvas.width - 2 * margin, canvas.height - 224f);
        var safe = Rect.MinMaxRect(canvas.xMin + margin, canvas.yMin + 24f, canvas.xMax - margin, canvas.yMax - 200f);
        var xs = new List<float> { safe.xMax - size, safe.xMin };
        var ys = new List<float> { safe.yMax - size, safe.yMin };
        foreach (Rect obstacle in obstacles)
        {
            xs.Add(Mathf.Clamp(obstacle.xMin - gap - size, safe.xMin, safe.xMax - size));
            xs.Add(Mathf.Clamp(obstacle.xMax + gap, safe.xMin, safe.xMax - size));
            ys.Add(Mathf.Clamp(obstacle.yMin - gap - size, safe.yMin, safe.yMax - size));
            ys.Add(Mathf.Clamp(obstacle.yMax + gap, safe.yMin, safe.yMax - size));
        }
        Rect best = new Rect(xs[0], ys[0], size, size);
        float score = float.MaxValue;
        foreach (float x in xs) foreach (float y in ys)
        {
            var candidate = new Rect(x, y, size, size);
            float overlap = 0;
            foreach (Rect obstacle in obstacles)
            {
                var padded = Rect.MinMaxRect(obstacle.xMin - gap, obstacle.yMin - gap, obstacle.xMax + gap, obstacle.yMax + gap);
                overlap += Mathf.Max(0, Mathf.Min(candidate.xMax, padded.xMax) - Mathf.Max(candidate.xMin, padded.xMin))
                    * Mathf.Max(0, Mathf.Min(candidate.yMax, padded.yMax) - Mathf.Max(candidate.yMin, padded.yMin));
            }
            float distance = (candidate.position - new Vector2(xs[0], ys[0])).sqrMagnitude;
            float value = overlap * 100000f + distance;
            if (value < score) { score = value; best = candidate; }
        }
        return best;
    }
}
