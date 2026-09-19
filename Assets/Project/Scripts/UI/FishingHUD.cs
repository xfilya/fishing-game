using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using VContainer;

public sealed class FishingHUD : MonoBehaviour
{
    [SerializeField] private GameObject _catchPanel;
    [SerializeField] private TextMeshProUGUI _catchName;
    [SerializeField] private TextMeshProUGUI _catchDetails;
    [SerializeField] private TextMeshProUGUI _catchStatus;
    [SerializeField] private TextMeshProUGUI _inventoryText;

    private readonly StringBuilder _textBuilder = new();
    private readonly Dictionary<string, int> _speciesCounts = new();
    private FishingController _fishingController;
    private ProgressService _progress;

    [Inject]
    public void Construct(FishingController fishingController, ProgressService progress)
    {
        _fishingController = fishingController;
        _progress = progress;
    }

    private void Start()
    {
        if (_fishingController == null || _progress == null || _catchPanel == null || _catchName == null || _catchDetails == null || _catchStatus == null || _inventoryText == null)
        {
            enabled = false;
            return;
        }

        _fishingController.CatchResolved += ShowCatch;
        _fishingController.ResultClosed += HideCatch;
        _progress.InventoryChanged += RefreshInventory;
        _catchPanel.SetActive(false);
        RefreshInventory();
    }

    private void OnDestroy()
    {
        if (_fishingController != null)
        {
            _fishingController.CatchResolved -= ShowCatch;
            _fishingController.ResultClosed -= HideCatch;
        }

        if (_progress != null)
            _progress.InventoryChanged -= RefreshInventory;
    }

    private void ShowCatch(CaughtFish caughtFish)
    {
        string color = GetRarityColor(caughtFish.Definition.Rarity);
        _catchName.text = caughtFish.Definition.DisplayName;
        _catchDetails.text = $"<color=#{color}>{GetRarityName(caughtFish.Definition.Rarity)}</color>  •  {caughtFish.Weight:0.00} кг";
        _catchStatus.text = caughtFish.IsNewSpecies ? "Новый вид отправлен в аквариум" : "Дубликат добавлен в инвентарь";
        _catchPanel.SetActive(true);
    }

    private void HideCatch()
    {
        _catchPanel.SetActive(false);
    }

    private void RefreshInventory()
    {
        _speciesCounts.Clear();

        foreach (CaughtFish caughtFish in _progress.Inventory)
        {
            string name = caughtFish.Definition.DisplayName;
            _speciesCounts[name] = _speciesCounts.TryGetValue(name, out int count) ? count + 1 : 1;
        }

        _textBuilder.Clear();

        if (_speciesCounts.Count == 0)
        {
            _textBuilder.Append("Дубликатов пока нет");
        }
        else
        {
            foreach (KeyValuePair<string, int> entry in _speciesCounts)
                _textBuilder.Append(entry.Key).Append("  ×").Append(entry.Value).AppendLine();
        }

        _inventoryText.text = _textBuilder.ToString().TrimEnd();
    }

    private static string GetRarityName(FishRarity rarity)
    {
        return rarity switch
        {
            FishRarity.Common => "Обычная",
            FishRarity.Rare => "Редкая",
            FishRarity.Epic => "Эпическая",
            FishRarity.Legendary => "Легендарная",
            _ => rarity.ToString()
        };
    }

    private static string GetRarityColor(FishRarity rarity)
    {
        return rarity switch
        {
            FishRarity.Common => "D7DEE8",
            FishRarity.Rare => "54B7FF",
            FishRarity.Epic => "C878FF",
            FishRarity.Legendary => "FFD05A",
            _ => "FFFFFF"
        };
    }
}
