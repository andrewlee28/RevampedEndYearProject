using UnityEngine;
using UnityEngine.UI;

public class LifeDisplay : MonoBehaviour
{
    // Drag your heart UI Image game objects into this array in the inspector
    public GameObject[] heartImages; 

    public void UpdateLivesDisplay(int currentLives)
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] != null)
            {
                // If our loop position is less than our remaining lives, turn the heart ON.
                // Otherwise, turn it OFF.
                heartImages[i].SetActive(i < currentLives);
            }
        }
    }
}