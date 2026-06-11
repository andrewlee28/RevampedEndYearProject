using UnityEngine;

public class SpawnZone : MonoBehaviour
{
    // Each individual platform can have its own width and height now!
    public float zoneWidth = 4f;
    public float zoneHeight = 0.5f;

    // This moves the yellow box drawing directly to the platform object itself
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(zoneWidth, zoneHeight, 0f));
    }
}