using UnityEngine;

public class Bullet : MonoBehaviour
{
    [HideInInspector] public Vector2 startPosition;
    [HideInInspector] public float knockbackForce;
    [HideInInspector] public bool distanceBasedKnockback;
    [HideInInspector] public float maxKnockbackRange;
    [HideInInspector] public float maxKnockbackMultiplier;
}