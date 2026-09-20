using TMPro;
using UnityEngine;
using VContainer;

public sealed class ShopInteractionController : MonoBehaviour
{
    [SerializeField] private GameObject _promptRoot;
    [SerializeField] private TextMeshProUGUI _promptText;

    private IInputService _input;
    private Player _player;
    private FishingController _fishingController;
    private ShopUIController _shopUI;
    private UIService _uiService;
    private ShopInteractable[] _shops;
    private ShopInteractable _nearestShop;

    [Inject]
    public void Construct(IInputService input, Player player, FishingController fishingController, ShopUIController shopUI, UIService uiService)
    {
        _input = input;
        _player = player;
        _fishingController = fishingController;
        _shopUI = shopUI;
        _uiService = uiService;
    }

    private void Start()
    {
        _shops = FindObjectsByType<ShopInteractable>(FindObjectsInactive.Include);
        SetPrompt(false);
    }

    private void Update()
    {
        if (_input == null || _shopUI == null)
            return;

        if (_shopUI.IsOpen || _uiService.IsMenuOpen)
        {
            SetPrompt(false);
            return;
        }

        FindNearestShop();
        SetPrompt(_nearestShop != null);

        if (_nearestShop != null && _input.InteractPressedThisFrame)
            _shopUI.Open(_nearestShop.ShopType);
    }

    private void FindNearestShop()
    {
        _nearestShop = null;

        if (_player == null || _fishingController == null || _fishingController.State != FishingState.Ready || _shops == null)
            return;

        float nearestDistance = float.MaxValue;

        foreach (ShopInteractable shop in _shops)
        {
            if (shop == null || !shop.isActiveAndEnabled)
                continue;

            float distance = Vector3.Distance(_player.transform.position, shop.InteractionPosition);

            if (distance > shop.InteractionRadius || distance >= nearestDistance)
                continue;

            nearestDistance = distance;
            _nearestShop = shop;
        }
    }

    private void SetPrompt(bool isVisible)
    {
        if (_promptRoot != null && _promptRoot.activeSelf != isVisible)
            _promptRoot.SetActive(isVisible);

        if (isVisible && _promptText != null)
            _promptText.text = $"<b>E</b>  {_nearestShop.DisplayName}";
    }
}
