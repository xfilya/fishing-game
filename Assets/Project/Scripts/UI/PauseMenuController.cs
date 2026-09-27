using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public sealed class PauseMenuController : MonoBehaviour
{
    [SerializeField] private GameObject _settingsPanel;
    [SerializeField] private GameObject _adminPanel;
    [SerializeField] private GameObject _exitPanel;
    [SerializeField] private GameObject _resetConfirmation;
    [SerializeField] private UnityEngine.UI.Button _continueButton;
    [SerializeField] private UnityEngine.UI.Button _settingsButton;
    [SerializeField] private UnityEngine.UI.Button _adminButton;
    [SerializeField] private UnityEngine.UI.Button _exitButton;
    [SerializeField] private UnityEngine.UI.Button _addCoinsButton;
    [SerializeField] private UnityEngine.UI.Button _removeCoinsButton;
    [SerializeField] private UnityEngine.UI.Button _zeroCoinsButton;
    [SerializeField] private UnityEngine.UI.Button _resetProgressButton;
    [SerializeField] private UnityEngine.UI.Button _confirmResetButton;
    [SerializeField] private UnityEngine.UI.Button _cancelResetButton;
    [SerializeField] private TMP_InputField _coinsInput;
    [SerializeField] private TextMeshProUGUI _balanceText;
    [SerializeField] private TextMeshProUGUI _statusText;
    [SerializeField] private Slider _masterVolume;
    [SerializeField] private Slider _musicVolume;
    [SerializeField] private Slider _ambienceVolume;
    [SerializeField] private Slider _effectsVolume;

    private EconomyService _economy;
    private PauseController _pauseController;
    private AudioService _audio;

    [Inject]
    public void Construct(EconomyService economy, PauseController pauseController, AudioService audio)
    {
        _economy = economy;
        _pauseController = pauseController;
        _audio = audio;
    }

    private void Start()
    {
        _continueButton.onClick.AddListener(_pauseController.Resume);
        _settingsButton.onClick.AddListener(ShowSettings);
        _adminButton.onClick.AddListener(ShowAdmin);
        _exitButton.onClick.AddListener(ShowExit);
        _addCoinsButton.onClick.AddListener(AddCoins);
        _removeCoinsButton.onClick.AddListener(RemoveCoins);
        _zeroCoinsButton.onClick.AddListener(ZeroCoins);
        _resetProgressButton.onClick.AddListener(OpenResetConfirmation);
        _confirmResetButton.onClick.AddListener(ResetAllProgress);
        _cancelResetButton.onClick.AddListener(CloseResetConfirmation);
        _economy.BalanceChanged += RefreshBalance;
        BindVolume(_masterVolume, _audio.MasterVolume, _audio.SetMasterVolume);
        BindVolume(_musicVolume, _audio.MusicVolume, _audio.SetMusicVolume);
        BindVolume(_ambienceVolume, _audio.AmbienceVolume, _audio.SetAmbienceVolume);
        BindVolume(_effectsVolume, _audio.EffectsVolume, _audio.SetEffectsVolume);

        foreach (Button button in GetComponentsInChildren<Button>(true))
            button.onClick.AddListener(_audio.PlayClick);

        ShowSettings();
        RefreshBalance(_economy.Coins);
    }

    private void OnEnable()
    {
        if (_settingsPanel != null)
            ShowSettings();
    }

    private void OnDestroy()
    {
        if (_economy != null)
            _economy.BalanceChanged -= RefreshBalance;
    }

    private void ShowSettings()
    {
        SetPanel(_settingsPanel);
    }

    private void ShowAdmin()
    {
        SetPanel(_adminPanel);
        RefreshBalance(_economy.Coins);
    }

    private void ShowExit()
    {
        SetPanel(_exitPanel);
    }

    private void SetPanel(GameObject activePanel)
    {
        _settingsPanel.SetActive(activePanel == _settingsPanel);
        _adminPanel.SetActive(activePanel == _adminPanel);
        _exitPanel.SetActive(activePanel == _exitPanel);
        _resetConfirmation.SetActive(false);
        SetStatus(string.Empty);
    }

    private void AddCoins()
    {
        if (!TryGetAmount(out int amount))
            return;

        _economy.AddCoins(amount);
        SetStatus($"Начислено {amount:N0} монет");
    }

    private void RemoveCoins()
    {
        if (!TryGetAmount(out int amount))
            return;

        int removed = Mathf.Min(amount, _economy.Coins);
        _economy.RemoveCoins(amount);
        SetStatus($"Списано {removed:N0} монет");
    }

    private void ZeroCoins()
    {
        _economy.SetCoins(0);
        SetStatus("Баланс обнулён");
    }

    private bool TryGetAmount(out int amount)
    {
        if (int.TryParse(_coinsInput.text, out amount) && amount > 0)
            return true;

        amount = 0;
        SetStatus("Введите целое число больше нуля");
        return false;
    }

    private void OpenResetConfirmation()
    {
        _resetConfirmation.SetActive(true);
    }

    private void CloseResetConfirmation()
    {
        _resetConfirmation.SetActive(false);
    }

    private void ResetAllProgress()
    {
        _economy.ResetAllProgress();
        _coinsInput.text = string.Empty;
        _resetConfirmation.SetActive(false);
        SetStatus("Весь прогресс сброшен");
    }

    private void RefreshBalance(int coins)
    {
        if (_balanceText != null)
            _balanceText.text = coins.ToString("N0");
    }

    private void SetStatus(string message)
    {
        if (_statusText != null)
            _statusText.text = message;
    }

    private void BindVolume(Slider slider, float value, UnityEngine.Events.UnityAction<float> setter)
    {
        if (slider == null)
            return;

        slider.SetValueWithoutNotify(value);
        slider.onValueChanged.AddListener(setter);
    }
}
