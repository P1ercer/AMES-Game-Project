using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [Header("Crosshair Parts")]
    public GameObject crosshairPart1;
    public GameObject crosshairPart2;

    public void SetCrosshairVisible(bool visible)
    {
        if (crosshairPart1 != null)
            crosshairPart1.SetActive(visible);

        if (crosshairPart2 != null)
            crosshairPart2.SetActive(visible);
    }
}