using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public int keyCount = 0; // Toplanan anahtar sayısı

    public void AddKey()
    {
        keyCount++;
        Debug.Log("Anahtar toplandı! Cepte: " + keyCount);
    }

    public bool HasKey()
    {
        return keyCount > 0;
    }

    public void UseKey()
    {
        if (keyCount > 0) keyCount--;
    }
}