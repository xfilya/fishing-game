using System;
using TMPro;
using UnityEngine;

public sealed class ShopItemView : MonoBehaviour
{
    private UnityEngine.UI.Image _background;
    private UnityEngine.UI.Image _icon;
    private UnityEngine.UI.Image _swatch;
    private TextMeshProUGUI _title;
    private TextMeshProUGUI _details;
    private UnityEngine.UI.Button _actionButton;
    private UnityEngine.UI.Image _actionBackground;
    private TextMeshProUGUI _actionText;

    public static ShopItemView Create(Transform parent, TMP_FontAsset font)
    {
        GameObject root = new("ShopItem", typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.LayoutElement), typeof(ShopItemView));
        root.transform.SetParent(parent, false);
        ShopItemView view = root.GetComponent<ShopItemView>();
        view.Build(font);
        return view;
    }

    public void Configure(Sprite icon, Color iconColor, string title, string details, string actionText, bool interactable, Action action)
    {
        _icon.sprite = icon;
        _icon.color = iconColor;
        _icon.enabled = icon != null;
        _swatch.color = iconColor;
        _title.text = title;
        _details.text = details;
        _actionText.text = actionText;
        _actionButton.interactable = interactable;
        _actionButton.onClick.RemoveAllListeners();

        if (action != null)
            _actionButton.onClick.AddListener(() => action());

        _actionBackground.color = interactable ? new Color(0.08f, 0.68f, 0.78f, 1f) : new Color(0.17f, 0.23f, 0.29f, 1f);
        _actionText.color = interactable ? Color.white : new Color(0.62f, 0.69f, 0.75f, 1f);
    }

    private void Build(TMP_FontAsset font)
    {
        _background = GetComponent<UnityEngine.UI.Image>();
        _background.color = new Color(0.045f, 0.086f, 0.125f, 0.96f);
        UnityEngine.UI.LayoutElement layout = GetComponent<UnityEngine.UI.LayoutElement>();
        layout.preferredHeight = 112f;
        layout.minHeight = 112f;

        GameObject iconPlate = CreateRect("IconPlate", transform, new Vector2(82f, 82f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(54f, 0f));
        UnityEngine.UI.Image plateImage = iconPlate.AddComponent<UnityEngine.UI.Image>();
        plateImage.color = new Color(0.075f, 0.15f, 0.2f, 1f);
        plateImage.raycastTarget = false;

        GameObject iconObject = CreateStretch("Icon", iconPlate.transform, 10f);
        _icon = iconObject.AddComponent<UnityEngine.UI.Image>();
        _icon.preserveAspect = true;
        _icon.raycastTarget = false;

        GameObject swatchObject = CreateRect("Swatch", transform, new Vector2(8f, 82f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(106f, 0f));
        _swatch = swatchObject.AddComponent<UnityEngine.UI.Image>();
        _swatch.raycastTarget = false;

        _title = CreateText("Title", transform, font, 25f, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
        RectTransform titleRect = _title.rectTransform;
        titleRect.anchorMin = new Vector2(0f, 0.5f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.offsetMin = new Vector2(128f, 0f);
        titleRect.offsetMax = new Vector2(-205f, -8f);
        _title.color = Color.white;

        _details = CreateText("Details", transform, font, 17f, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
        RectTransform detailsRect = _details.rectTransform;
        detailsRect.anchorMin = new Vector2(0f, 0f);
        detailsRect.anchorMax = new Vector2(1f, 0.52f);
        detailsRect.offsetMin = new Vector2(128f, 8f);
        detailsRect.offsetMax = new Vector2(-205f, 0f);
        _details.color = new Color(0.63f, 0.73f, 0.8f, 1f);

        GameObject buttonObject = CreateRect("Action", transform, new Vector2(174f, 58f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-102f, 0f));
        _actionBackground = buttonObject.AddComponent<UnityEngine.UI.Image>();
        _actionButton = buttonObject.AddComponent<UnityEngine.UI.Button>();
        _actionButton.targetGraphic = _actionBackground;
        UnityEngine.UI.ColorBlock colors = _actionButton.colors;
        colors.highlightedColor = new Color(0.2f, 0.86f, 0.95f, 1f);
        colors.pressedColor = new Color(0.04f, 0.52f, 0.62f, 1f);
        colors.disabledColor = Color.white;
        _actionButton.colors = colors;
        _actionText = CreateText("Label", buttonObject.transform, font, 19f, FontStyles.Bold, TextAlignmentOptions.Center);
        SetStretch(_actionText.rectTransform, 4f);
    }

    private static GameObject CreateRect(string objectName, Transform parent, Vector2 size, Vector2 anchorMin, Vector2 anchorMax, Vector2 position)
    {
        GameObject result = new(objectName, typeof(RectTransform));
        result.transform.SetParent(parent, false);
        RectTransform rect = (RectTransform)result.transform;
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        return result;
    }

    private static GameObject CreateStretch(string objectName, Transform parent, float inset)
    {
        GameObject result = new(objectName, typeof(RectTransform));
        result.transform.SetParent(parent, false);
        SetStretch((RectTransform)result.transform, inset);
        return result;
    }

    private static TextMeshProUGUI CreateText(string objectName, Transform parent, TMP_FontAsset font, float size, FontStyles style, TextAlignmentOptions alignment)
    {
        GameObject result = new(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        result.transform.SetParent(parent, false);
        TextMeshProUGUI text = result.GetComponent<TextMeshProUGUI>();
        text.font = font;
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;
        return text;
    }

    private static void SetStretch(RectTransform rect, float inset)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.one * inset;
        rect.offsetMax = Vector2.one * -inset;
    }
}
