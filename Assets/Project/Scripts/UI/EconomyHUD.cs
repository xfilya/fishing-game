using TMPro;
using UnityEngine;
using VContainer;

public sealed class EconomyHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _balanceText;

    private EconomyService _economy;

    [Inject]
    public void Construct(EconomyService economy)
    {
        _economy = economy;
    }

    private void Start()
    {
        if (_economy == null || _balanceText == null)
        {
            enabled = false;
            return;
        }

        _economy.BalanceChanged += Refresh;
        Refresh(_economy.Coins);
    }

    private void OnDestroy()
    {
        if (_economy != null)
            _economy.BalanceChanged -= Refresh;
    }

    private void Refresh(int coins)
    {
        _balanceText.text = coins.ToString("N0");
    }
}
