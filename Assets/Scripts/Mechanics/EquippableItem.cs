using UnityEngine;

public class EquippableItem : MonoBehaviour
{
    private bool equipped = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(equipped)
            return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerEquipment equipment = collision.gameObject.GetComponent<PlayerEquipment>();
            if(equipment != null)
            {
                equipped = true;
                equipment.EquipItem(gameObject);
            }
        }
    }
}
