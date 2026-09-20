using TMPro;
using UnityEngine;
using VContainer;

public sealed class ShopUIController : MonoBehaviour
{
    [SerializeField] private GameObject _shopRoot;
    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private TextMeshProUGUI _balanceText;
    [SerializeField] private UnityEngine.UI.Image _shopIcon;
    [SerializeField] private Transform _content;
    [SerializeField] private GameObject _emptyState;
    [SerializeField] private UnityEngine.UI.Button _closeButton;
    [SerializeField] private UnityEngine.UI.Button _sellAllButton;
    [SerializeField] private TextMeshProUGUI _sellAllText;
    [SerializeField] private GameObject _confirmationRoot;
    [SerializeField] private TextMeshProUGUI _confirmationText;
    [SerializeField] private UnityEngine.UI.Button _confirmButton;
    [SerializeField] private UnityEngine.UI.Button _cancelButton;
    [SerializeField] private TMP_FontAsset _font;
    [SerializeField] private Sprite _fishIcon;
    [SerializeField] private Sprite _rodIcon;
    [SerializeField] private Sprite _bobberIcon;

    private EconomyService _economy;
    private ProgressService _progress;
    private EquipmentCatalog _equipmentCatalog;
    private IInputService _input;
    private Player _player;
    private ShopType _currentShop;
    private bool _isOpen;
    private int _openedFrame;

    public bool IsOpen => _isOpen;

    [Inject]
    public void Construct(EconomyService economy, ProgressService progress, EquipmentCatalog equipmentCatalog, IInputService input, Player player)
    {
        _economy = economy;
        _progress = progress;
        _equipmentCatalog = equipmentCatalog;
        _input = input;
        _player = player;
    }

    private void Start()
    {
        if (_shopRoot == null || _content == null || _closeButton == null || _sellAllButton == null || _confirmationRoot == null)
        {
            enabled = false;
            return;
        }

        _closeButton.onClick.AddListener(Close);
        _sellAllButton.onClick.AddListener(OpenSellAllConfirmation);
        _confirmButton.onClick.AddListener(ConfirmSellAll);
        _cancelButton.onClick.AddListener(CloseConfirmation);
        _economy.BalanceChanged += OnBalanceChanged;
        _economy.EquipmentChanged += OnEquipmentChanged;
        _progress.InventoryChanged += OnInventoryChanged;
        _shopRoot.SetActive(false);
        _confirmationRoot.SetActive(false);
        RefreshBalance();
    }

    private void OnDestroy()
    {
        if (_economy != null)
        {
            _economy.BalanceChanged -= OnBalanceChanged;
            _economy.EquipmentChanged -= OnEquipmentChanged;
        }

        if (_progress != null)
            _progress.InventoryChanged -= OnInventoryChanged;

    }

    private void Update()
    {
        if (_isOpen && Time.frameCount != _openedFrame && _input.InteractPressedThisFrame)
            Close();
    }

    public void Open(ShopType shopType)
    {
        if (!enabled)
            return;

        _currentShop = shopType;
        _isOpen = true;
        _openedFrame = Time.frameCount;
        _shopRoot.SetActive(true);
        _confirmationRoot.SetActive(false);
        _input.SetGameplayEnabled(false);
        _player.EnableCamera(false);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Refresh();
    }

    public void Close()
    {
        if (!_isOpen)
            return;

        _isOpen = false;
        _confirmationRoot.SetActive(false);
        _shopRoot.SetActive(false);
        RestoreGameplay();
    }

    private void RestoreGameplay()
    {
        _input?.SetGameplayEnabled(true);
        if (_player != null)
            _player.EnableCamera(true);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Refresh()
    {
        ClearContent();
        RefreshBalance();

        switch (_currentShop)
        {
            case ShopType.FishBuyer:
                RefreshFishBuyer();
                break;
            case ShopType.RodSeller:
                RefreshEquipment(EquipmentType.Rod);
                break;
            case ShopType.BobberSeller:
                RefreshEquipment(EquipmentType.Bobber);
                break;
        }
    }

    private void RefreshFishBuyer()
    {
        _title.text = "СКУПЩИК РЫБЫ";
        _shopIcon.sprite = _fishIcon;
        _shopIcon.color = Color.white;
        int total = 0;

        foreach (CaughtFish fish in _progress.Inventory)
        {
            CaughtFish currentFish = fish;
            int price = _economy.GetFishPrice(currentFish);
            total += price;
            ShopItemView view = ShopItemView.Create(_content, _font);
            string details = $"{GetRarityName(currentFish.Definition.Rarity)}  •  {currentFish.Weight:0.00} кг";
            view.Configure(_fishIcon, FishRarityColors.Get(currentFish.Definition.Rarity), currentFish.Definition.DisplayName, details, $"+ {price:N0}", true, () => SellFish(currentFish));
        }

        bool hasFish = _progress.Inventory.Count > 0;
        _emptyState.SetActive(!hasFish);
        _sellAllButton.gameObject.SetActive(hasFish);
        _sellAllText.text = $"ПРОДАТЬ ВСЁ  + {total:N0}";
    }

    private void RefreshEquipment(EquipmentType type)
    {
        bool isRod = type == EquipmentType.Rod;
        _title.text = isRod ? "МАГАЗИН УДОЧЕК" : "МАГАЗИН ПОПЛАВКОВ";
        _shopIcon.sprite = isRod ? _rodIcon : _bobberIcon;
        _shopIcon.color = Color.white;
        _emptyState.SetActive(false);
        _sellAllButton.gameObject.SetActive(false);

        foreach (EquipmentDefinition item in _equipmentCatalog.GetItems(type))
        {
            EquipmentDefinition currentItem = item;
            bool owned = _economy.IsOwned(currentItem.Id);
            bool equipped = _economy.IsEquipped(currentItem.Id);
            bool previousOwned = _economy.IsPreviousTierOwned(currentItem);
            bool canPurchase = _economy.CanPurchase(currentItem);
            bool interactable = owned ? !equipped : canPurchase;
            string action = GetEquipmentAction(currentItem, owned, equipped, previousOwned);
            string details = isRod ? $"Удача +{currentItem.RarityLuck:0}  •  Крупный вес ×{currentItem.WeightBias:0.##}" : $"Удача редкости +{currentItem.RarityLuck:0}";
            ShopItemView view = ShopItemView.Create(_content, _font);
            view.Configure(isRod ? _rodIcon : _bobberIcon, currentItem.Color, currentItem.DisplayName, details, action, interactable, () => PurchaseOrEquip(currentItem));
        }
    }

    private string GetEquipmentAction(EquipmentDefinition item, bool owned, bool equipped, bool previousOwned)
    {
        if (equipped)
            return "ВЫБРАНО";

        if (owned)
            return "ВЫБРАТЬ";

        if (!previousOwned)
            return "ЗАКРЫТО";

        return item.Price.ToString("N0");
    }

    private void SellFish(CaughtFish fish)
    {
        _economy.TrySell(fish, out _);
    }

    private void PurchaseOrEquip(EquipmentDefinition item)
    {
        if (_economy.IsOwned(item.Id))
            _economy.TryEquip(item);
        else
            _economy.TryPurchase(item);
    }

    private void OpenSellAllConfirmation()
    {
        int total = 0;

        foreach (CaughtFish fish in _progress.Inventory)
            total += _economy.GetFishPrice(fish);

        if (total <= 0)
            return;

        _confirmationText.text = $"Продать все дубликаты за {total:N0} монет?\nКоллекционные рыбы останутся в аквариуме.";
        _confirmationRoot.SetActive(true);
    }

    private void ConfirmSellAll()
    {
        _confirmationRoot.SetActive(false);
        _economy.SellAll();
    }

    private void CloseConfirmation()
    {
        _confirmationRoot.SetActive(false);
    }

    private void OnBalanceChanged(int _)
    {
        RefreshBalance();

        if (_isOpen && _currentShop != ShopType.FishBuyer)
            Refresh();
    }

    private void OnEquipmentChanged(EquipmentDefinition _)
    {
        if (_isOpen && _currentShop != ShopType.FishBuyer)
            Refresh();
    }

    private void OnInventoryChanged()
    {
        if (_isOpen && _currentShop == ShopType.FishBuyer)
            Refresh();
    }

    private void RefreshBalance()
    {
        if (_balanceText != null && _economy != null)
            _balanceText.text = _economy.Coins.ToString("N0");
    }

    private void ClearContent()
    {
        for (int i = _content.childCount - 1; i >= 0; i--)
            Destroy(_content.GetChild(i).gameObject);
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

}
