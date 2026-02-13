using UnityEngine;
using System.Collections.Generic; // Listeleri kullanmak için bunu eklemeliyiz

public class PlayerInventory : MonoBehaviour
{
    // ==========================================================
    // BÖLÜM 1: KAPI ANAHTARLARI (Eski Sistem - Dokunmadık)
    // ==========================================================
    [Header("Kapı Anahtarları")]
    public int keyCount = 0; 

    public void AddKey()
    {
        keyCount++;
        Debug.Log("Kapı Anahtarı toplandı! Cepte: " + keyCount);
    }

    public bool HasKey()
    {
        return keyCount > 0;
    }

    public void UseKey()
    {
        if (keyCount > 0) keyCount--;
        Debug.Log("Anahtar kullanıldı. Kalan: " + keyCount);
    }

    // ==========================================================
    // BÖLÜM 2: GÖREV EŞYALARI (Mezarlar İçin Yeni Sistem)
    // ==========================================================
    [Header("Görev Eşyaları")]
    // Toplanan eşyaların isimlerini burada saklayacağız
    // Örn: ["Oyuncak Ayı", "Köstekli Saat", "Eski Kolye"]
    public List<string> questItems = new List<string>();

    // Yerden eşya alınca çalışacak
    public void AddQuestItem(string itemName)
    {
        questItems.Add(itemName);
        Debug.Log("Görev Eşyası Alındı: " + itemName);
    }

    // Mezara gelince "Bende o eşya var mı?" diye sormak için
    public bool HasQuestItem(string itemName)
    {
        return questItems.Contains(itemName);
    }

    // Mezara eşyayı verince envanterden silmek için
    public void RemoveQuestItem(string itemName)
    {
        if (questItems.Contains(itemName))
        {
            questItems.Remove(itemName);
            Debug.Log("Eşya Mezara Bırakıldı: " + itemName);
        }
    }
}