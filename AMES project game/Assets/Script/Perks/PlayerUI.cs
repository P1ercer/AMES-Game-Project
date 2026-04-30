using UnityEngine;
using AmesGame;

public class PlayerUI : MonoBehaviour
{
    [Header("Crosshair Parts")]
    public GameObject crosshairPart1;
    public GameObject crosshairPart2;
        [Header("Key UI")]
        public UnityEngine.UI.Image keyImage;

        // update the key UI image visibility
        public void SetHasKey(bool hasKey)
        {
            if (keyImage != null)
                keyImage.enabled = hasKey;
        }

    private void Start()
    {
        // hide key UI by default
        if (keyImage != null)
            keyImage.enabled = false;

        // if player already has a KeyHolder (e.g. loaded state), reflect that
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            var kh = playerObj.GetComponent<KeyHolder>();
            if (kh != null)
                SetHasKey(kh.HasKey);
        }
    }

    public void SetCrosshairVisible(bool visible)
    {
        if (crosshairPart1 != null)
            crosshairPart1.SetActive(visible);

        if (crosshairPart2 != null)
            crosshairPart2.SetActive(visible);
    }
}