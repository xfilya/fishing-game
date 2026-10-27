using DG.Tweening;
using TMPro;
using UnityEngine;
using VContainer;

[DefaultExecutionOrder(200)]
public sealed class MainMenuController : MonoBehaviour
{
    [SerializeField] private CanvasGroup _menuRoot;
    [SerializeField] private GameObject _homePanel;
    [SerializeField] private GameObject _settingsPanel;
    [SerializeField] private GameObject _resetConfirmation;
    [SerializeField] private Transform _cameraPosition;
    [SerializeField] private Camera _camera;
    [SerializeField] private GameObject _rodRoot;
    [SerializeField] private GameObject _playerVisuals;
    [SerializeField] private GameObject[] _gameplayUI;
    [SerializeField] private UnityEngine.UI.Button _playButton;
    [SerializeField] private UnityEngine.UI.Button _settingsButton;
    [SerializeField] private UnityEngine.UI.Button _backButton;
    [SerializeField] private UnityEngine.UI.Button _resetButton;
    [SerializeField] private UnityEngine.UI.Button _confirmResetButton;
    [SerializeField] private UnityEngine.UI.Button _cancelResetButton;
    [SerializeField] private UnityEngine.UI.Slider _masterVolume;
    [SerializeField] private UnityEngine.UI.Slider _musicVolume;
    [SerializeField] private UnityEngine.UI.Slider _ambienceVolume;
    [SerializeField] private UnityEngine.UI.Slider _effectsVolume;
    [SerializeField] private TextMeshProUGUI _progressText;
    [SerializeField] private TextMeshProUGUI _controlsText;
    [SerializeField, Min(0.1f)] private float _flightDuration = 2.6f;
    [SerializeField, Min(0f)] private float _flightHeight = 3f;
    [SerializeField, Range(20f, 90f)] private float _menuFieldOfView = 55f;

    private IInputService _input;
    private UIService _uiService;
    private Player _player;
    private PauseController _pauseController;
    private FishingController _fishing;
    private ShopUIController _shop;
    private AudioService _audio;
    private SaveService _save;
    private ProgressService _progress;
    private EconomyService _economy;
    private FishCatalog _catalog;
    private Transform _cameraParent;
    private Vector3 _cameraLocalPosition;
    private Quaternion _cameraLocalRotation;
    private float _gameplayFieldOfView;
    private bool _playerVisualsActive;
    private bool _rodActive;
    private Tween _cameraTween;
    private bool _wasTouch;

    public bool IsTransitioning { get; private set; }

    [Inject]
    public void Construct(IInputService input, UIService uiService, Player player, PauseController pauseController, FishingController fishing, ShopUIController shop, AudioService audio, SaveService save, ProgressService progress, EconomyService economy, FishCatalog catalog)
    {
        _input = input;
        _uiService = uiService;
        _player = player;
        _pauseController = pauseController;
        _fishing = fishing;
        _shop = shop;
        _audio = audio;
        _save = save;
        _progress = progress;
        _economy = economy;
        _catalog = catalog;
    }

    private void Start()
    {
        _cameraParent = _camera.transform.parent;
        _cameraLocalPosition = _camera.transform.localPosition;
        _cameraLocalRotation = _camera.transform.localRotation;
        _gameplayFieldOfView = _camera.fieldOfView;
        _playerVisualsActive = _playerVisuals.activeSelf;
        _rodActive = _rodRoot.activeSelf;
        _playButton.onClick.AddListener(Play);
        _settingsButton.onClick.AddListener(ShowSettings);
        _backButton.onClick.AddListener(ShowHome);
        _resetButton.onClick.AddListener(OpenResetConfirmation);
        _confirmResetButton.onClick.AddListener(ResetProgress);
        _cancelResetButton.onClick.AddListener(CloseResetConfirmation);
        BindVolume(_masterVolume, _audio.SetMasterVolume);
        BindVolume(_musicVolume, _audio.SetMusicVolume);
        BindVolume(_ambienceVolume, _audio.SetAmbienceVolume);
        BindVolume(_effectsVolume, _audio.SetEffectsVolume);

        foreach (UnityEngine.UI.Button button in _menuRoot.GetComponentsInChildren<UnityEngine.UI.Button>(true))
            button.onClick.AddListener(_audio.PlayClick);

        Open();
    }

    private void Update()
    {
        if (_wasTouch != _input.IsTouch)
            RefreshControlsHint();

        if (!_uiService.IsMainMenuOpen || IsTransitioning || !_input.EscapePressedThisFrame)
            return;

        if (_resetConfirmation.activeSelf)
            CloseResetConfirmation();
        else
            ShowHome();
    }

    private void OnDestroy()
    {
        _cameraTween?.Kill();
        if (_camera != null && _cameraParent != null)
            _camera.transform.SetParent(_cameraParent, false);
    }

    public void Open()
    {
        if (_uiService.IsMainMenuOpen && !IsTransitioning)
        {
            ShowHome();
            return;
        }

        _cameraTween?.Kill();
        IsTransitioning = false;
        _uiService.SetMainMenuOpen(true);
        _shop.Close();
        _pauseController.Resume();
        _input.SetGameplayEnabled(false);
        _player.SetGameplayEnabled(false);
        _fishing.CancelFishing();
        _save.Save();
        SetGameplayVisible(false);
        _camera.transform.SetParent(null, true);
        _camera.transform.SetPositionAndRotation(_cameraPosition.position, _cameraPosition.rotation);
        _camera.fieldOfView = _menuFieldOfView;
        _menuRoot.gameObject.SetActive(true);
        _menuRoot.alpha = 1f;
        _menuRoot.interactable = true;
        _menuRoot.blocksRaycasts = true;
        ShowHome();
    }

    public void Play()
    {
        if (IsTransitioning || !_uiService.IsMainMenuOpen)
            return;

        IsTransitioning = true;
        _menuRoot.interactable = false;
        _menuRoot.blocksRaycasts = false;
        Cursor.visible = false;
        Vector3 startPosition = _camera.transform.position;
        Quaternion startRotation = _camera.transform.rotation;
        Vector3 targetPosition = _cameraParent.TransformPoint(_cameraLocalPosition);
        Quaternion targetRotation = _cameraParent.rotation * _cameraLocalRotation;
        Vector3 controlPosition = (startPosition + targetPosition) * 0.5f + Vector3.up * _flightHeight;

        _cameraTween = DOVirtual.Float(0f, 1f, _flightDuration, progress =>
        {
            Vector3 first = Vector3.Lerp(startPosition, controlPosition, progress);
            Vector3 second = Vector3.Lerp(controlPosition, targetPosition, progress);
            _camera.transform.SetPositionAndRotation(Vector3.Lerp(first, second, progress), Quaternion.Slerp(startRotation, targetRotation, progress));
            _camera.fieldOfView = Mathf.Lerp(_menuFieldOfView, _gameplayFieldOfView, progress);
            _menuRoot.alpha = 1f - Mathf.Clamp01(progress * 5f);
        }).SetEase(Ease.InOutSine).SetUpdate(true).OnComplete(CompleteFlight);
    }

    private void CompleteFlight()
    {
        _cameraTween = null;
        _camera.transform.SetParent(_cameraParent, false);
        _camera.transform.SetLocalPositionAndRotation(_cameraLocalPosition, _cameraLocalRotation);
        _camera.fieldOfView = _gameplayFieldOfView;
        _menuRoot.gameObject.SetActive(false);
        IsTransitioning = false;
        _uiService.SetMainMenuOpen(false);
        SetGameplayVisible(true);
        _player.SetGameplayEnabled(true);
        _input.SetGameplayEnabled(true);
    }

    private void SetGameplayVisible(bool isVisible)
    {
        foreach (GameObject root in _gameplayUI)
            root.SetActive(isVisible);

        _rodRoot.SetActive(isVisible && _rodActive);
        _playerVisuals.SetActive(isVisible && _playerVisualsActive);
    }

    private void ShowHome()
    {
        _homePanel.SetActive(true);
        _settingsPanel.SetActive(false);
        _resetConfirmation.SetActive(false);
        _progressText.text = $"Коллекция: {_progress.Collection.Count} / {_catalog.Fish.Count}  |  Дубликаты: {_progress.Inventory.Count}";
        RefreshControlsHint();
    }

    private void RefreshControlsHint()
    {
        _wasTouch = _input.IsTouch;
        _controlsText.text = _wasTouch ? "Джойстик - движение | справа - обзор\nКнопки - удочка, торговля и пауза" : "WASD - движение | мышь - обзор\nЛКМ - удочка | E - торговля | Esc - пауза";
    }

    private void ShowSettings()
    {
        _homePanel.SetActive(false);
        _settingsPanel.SetActive(true);
        _resetConfirmation.SetActive(false);
        _masterVolume.SetValueWithoutNotify(_audio.MasterVolume);
        _musicVolume.SetValueWithoutNotify(_audio.MusicVolume);
        _ambienceVolume.SetValueWithoutNotify(_audio.AmbienceVolume);
        _effectsVolume.SetValueWithoutNotify(_audio.EffectsVolume);
    }

    private void OpenResetConfirmation() => _resetConfirmation.SetActive(true);
    private void CloseResetConfirmation() => _resetConfirmation.SetActive(false);

    private void ResetProgress()
    {
        _economy.ResetAllProgress();
        CloseResetConfirmation();
    }

    private void BindVolume(UnityEngine.UI.Slider slider, UnityEngine.Events.UnityAction<float> setter) => slider.onValueChanged.AddListener(setter);
}
