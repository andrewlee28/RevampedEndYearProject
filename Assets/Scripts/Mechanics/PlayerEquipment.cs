using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    public Transform equipPoint;
    private GameObject currentItem;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void EquipItem(GameObject newItem)
    {
        if(currentItem != null)
        {
            Destroy(currentItem);
        }
        currentItem = newItem;

        // Disable physics
        Rigidbody2D rb = newItem.GetComponent<Rigidbody2D>();

        if(rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // Disable collider
        Collider2D col = newItem.GetComponent<Collider2D>();

        if(col != null)
        {
            col.enabled = false;
        }

        // Parent to player
        newItem.transform.SetParent(equipPoint);

        // Set into position
        newItem.transform.localPosition = Vector3.zero;
        newItem.transform.localRotation = Quaternion.identity;
    }
}
