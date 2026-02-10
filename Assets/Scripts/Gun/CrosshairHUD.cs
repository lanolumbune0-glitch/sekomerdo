using UnityEngine;
using UnityEngine.UI;

public class CrosshairHUD : MonoBehaviour
{
    public Image reloadRing; // Halka resmini buraya atacağız

    void Start()
    {
        if (reloadRing != null) reloadRing.fillAmount = 0f; // Başlangıçta gizle
    }

    public void UpdateReloadProgress(float progress)
    {
        if (reloadRing != null)
        {
            reloadRing.fillAmount = progress;
        }
    }

    public void HideReloadRing()
    {
        if (reloadRing != null)
        {
            reloadRing.fillAmount = 0f;
        }
    }
}