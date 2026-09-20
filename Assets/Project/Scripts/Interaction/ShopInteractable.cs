using UnityEngine;

public sealed class ShopInteractable : MonoBehaviour
{
    [SerializeField] private ShopType _shopType;
    [SerializeField] private string _displayName = "Торговец";
    [SerializeField, Min(0.5f)] private float _interactionRadius = 3f;
    [SerializeField] private Transform _interactionPoint;

    public ShopType ShopType => _shopType;
    public string DisplayName => _displayName;
    public float InteractionRadius => _interactionRadius;
    public Vector3 InteractionPosition => _interactionPoint != null ? _interactionPoint.position : transform.position;

    public void Configure(ShopType shopType, string displayName, float interactionRadius)
    {
        _shopType = shopType;
        _displayName = displayName;
        _interactionRadius = Mathf.Max(0.5f, interactionRadius);
    }
}
