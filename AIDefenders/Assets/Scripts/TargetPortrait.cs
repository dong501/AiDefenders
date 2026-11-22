using UnityEngine;
using UnityEngine.UI;

public static class TargetPortrait
{
    public static RawImage Add(Transform parent, Target_Base target, Vector2 anchor, Vector2 position, float size)
    {
        var frame = UiFont.AddImage(parent, "Portrait_" + target.name, new Color32(27, 44, 58, 255));
        UiSkin.Place(frame.rectTransform, anchor, position, Vector2.one * size);
        frame.raycastTarget = false;
        var image = new GameObject("Model", typeof(RectTransform), typeof(RawImage)).GetComponent<RawImage>();
        image.transform.SetParent(frame.transform, false);
        UiFont.Stretch(image.rectTransform);
        image.texture = Resources.Load<Texture2D>("UI/TargetPortraits/" + target.name);
        image.raycastTarget = false;
        return image;
    }
}
