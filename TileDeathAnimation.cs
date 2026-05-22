using System.Collections;
using UnityEngine;

public class TileDeathAnimation : MonoBehaviour
{
    [SerializeField] private float duration = 0.3f;
    [SerializeField] private Vector3 rotationDegrees = new Vector3(0f, 0f, 180f);
    [SerializeField] private AnimationCurve easing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    public IEnumerator Play(Tile tile)
    {
        Transform target = tile != null ? tile.transform : transform;
        Quaternion startRotation = target.localRotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(rotationDegrees);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
            target.localRotation = Quaternion.Slerp(startRotation, endRotation, easing.Evaluate(progress));
            yield return null;
        }

        target.localRotation = endRotation;
    }
}
