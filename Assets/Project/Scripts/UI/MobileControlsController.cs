using TMPro;
using UnityEngine;
using VContainer;

public sealed class MobileControlsController : MonoBehaviour
{
    [SerializeField] private GameObject _controlsRoot;
    [SerializeField] private MobileJoystick _joystick;
    [SerializeField] private MobileLookArea _lookArea;
    [SerializeField] private MobileActionButton _primaryButton;
    [SerializeField] private MobileActionButton _jumpButton;
    [SerializeField] private MobileActionButton _sprintButton;
    [SerializeField] private MobileActionButton _interactButton;
    [SerializeField] private MobileActionButton _pauseButton;
    [SerializeField] private TextMeshProUGUI _primaryLabel;
    [SerializeField] private TextMeshProUGUI _sprintLabel;
    [SerializeField] private bool _simulateTouchInEditor;

    private IInputService _input;
    private MobileInputService _mobile;
    private UIService _ui;
    private ShopUIController _shop;
    private ShopInteractionController _interaction;
    private FishingController _fishing;
    private bool _wasTouch;

    [Inject]
    public void Construct(IInputService input, MobileInputService mobile, UIService ui, ShopUIController shop, ShopInteractionController interaction, FishingController fishing)
    {
        _input = input;
        _mobile = mobile;
        _ui = ui;
        _shop = shop;
        _interaction = interaction;
        _fishing = fishing;
    }

    private void Start()
    {
        _mobile.SetEditorSimulation(_simulateTouchInEditor);
        _joystick.Initialize(_mobile);
        _lookArea.Initialize(_mobile);
        _primaryButton.Initialize(_mobile);
        _jumpButton.Initialize(_mobile);
        _sprintButton.Initialize(_mobile);
        _interactButton.Initialize(_mobile);
        _pauseButton.Initialize(_mobile);
    }

    private void LateUpdate()
    {
        bool isTouch = _input.IsTouch;
        if (_wasTouch != isTouch)
        {
            _wasTouch = isTouch;
            _ui.RefreshCursor();
        }

        bool isVisible = isTouch && _input.IsGameplayEnabled && !_ui.IsMenuOpen && !_shop.IsOpen;
        if (_controlsRoot.activeSelf != isVisible)
        {
            _controlsRoot.SetActive(isVisible);
            if (!isVisible)
                _mobile.ResetGameplay();
        }

        if (!isVisible)
            return;

        FishingState state = _fishing.State;
        bool canMove = state == FishingState.Ready || state == FishingState.Reeling || state == FishingState.Result;
        _joystick.gameObject.SetActive(canMove);
        _jumpButton.Button.interactable = canMove;
        _sprintButton.Button.interactable = canMove;
        _interactButton.gameObject.SetActive(_interaction.CanInteract);
        _primaryButton.Button.interactable = state != FishingState.Casting && state != FishingState.Reeling;
        _primaryLabel.text = state switch
        {
            FishingState.Waiting => "ВЫТЯНУТЬ",
            FishingState.BiteWindow => "ПОДСЕЧЬ",
            FishingState.Reeling => "ВЫТЯГИВАЕМ",
            FishingState.Result => "ПРОДОЛЖИТЬ",
            _ => "ЗАБРОС"
        };
        _primaryButton.Button.image.color = state == FishingState.BiteWindow ? new Color(0.95f, 0.64f, 0.17f, 0.95f) : new Color(0.08f, 0.58f, 0.68f, 0.9f);
        _sprintButton.Button.image.color = _mobile.SprintHeld ? new Color(0.12f, 0.62f, 0.67f, 0.95f) : new Color(0.04f, 0.14f, 0.18f, 0.8f);
        _sprintLabel.text = _mobile.SprintHeld ? "БЕГ ВКЛ." : "БЕГ";
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            ResetInput();
    }

    private void OnApplicationPause(bool isPaused)
    {
        if (isPaused)
            ResetInput();
    }

    private void ResetInput()
    {
        if (_mobile == null)
            return;

        _mobile.ResetGameplay();
        _joystick.ResetInput();
        _lookArea.ResetInput();
        _primaryButton.ResetInput();
        _jumpButton.ResetInput();
        _sprintButton.ResetInput();
        _interactButton.ResetInput();
        _pauseButton.ResetInput();
    }

    private void OnValidate()
    {
        if (Application.isPlaying && _mobile != null)
            _mobile.SetEditorSimulation(_simulateTouchInEditor);
    }
}
