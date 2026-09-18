using UnityEngine;

public sealed class AquariumSwimPoints : MonoBehaviour
{
    public int Count => transform.childCount;

    public Transform GetRandomPoint(Vector3 origin, Transform excludedPoint, float minimumDistance)
    {
        int count = transform.childCount;

        if (count == 0)
            return null;

        float minimumDistanceSqr = minimumDistance * minimumDistance;
        int startIndex = Random.Range(0, count);
        Transform fallback = null;

        for (int i = 0; i < count; i++)
        {
            Transform point = transform.GetChild((startIndex + i) % count);

            if (point == excludedPoint || !point.gameObject.activeInHierarchy)
                continue;

            fallback ??= point;

            if ((point.position - origin).sqrMagnitude >= minimumDistanceSqr)
                return point;
        }

        return fallback;
    }
}
