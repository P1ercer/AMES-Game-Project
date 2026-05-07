using UnityEngine;
using AmesGame;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class PlayerUI : MonoBehaviour
{
    [Header("Crosshair Parts")]
    public GameObject crosshairPart1;
    public GameObject crosshairPart2;

    [Header("Key UI")]
    public Image keyImage;

    [Header("Perk Cooldown UI (Q / CTRL / SHIFT)")]
    // Use icon images instead of TMP for perk name display
    public Image qPerkIcon;
    public TextMeshProUGUI qTimeText;
    public Image qCooldownBar;

    public Image ctrlPerkIcon;
    public TextMeshProUGUI ctrlTimeText;
    public Image ctrlCooldownBar;

    public Image shiftPerkIcon;
    public TextMeshProUGUI shiftTimeText;
    public Image shiftCooldownBar;

    private PerkController _perkController;

    private void Start()
    {
        // hide key UI by default
        if (keyImage != null)
            keyImage.enabled = false;

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            var kh = playerObj.GetComponent<KeyHolder>();
            if (kh != null)
                SetHasKey(kh.HasKey);

            _perkController = playerObj.GetComponent<PerkController>();
        }

        // ensure bars/icons start cleared
        ClearBar(qPerkIcon, qTimeText, qCooldownBar);
        ClearBar(ctrlPerkIcon, ctrlTimeText, ctrlCooldownBar);
        ClearBar(shiftPerkIcon, shiftTimeText, shiftCooldownBar);
    }

    private void Update()
    {
        if (_perkController == null) return;

        UpdateBarForKey(ActivationKey.Q, qPerkIcon, qTimeText, qCooldownBar);
        UpdateBarForKey(ActivationKey.Ctrl, ctrlPerkIcon, ctrlTimeText, ctrlCooldownBar);
        UpdateBarForKey(ActivationKey.Shift, shiftPerkIcon, shiftTimeText, shiftCooldownBar);
    }

    private void UpdateBarForKey(ActivationKey key, Image iconImage, TextMeshProUGUI timeText, Image bar)
    {
        if (iconImage == null || timeText == null || bar == null) return;

        // find first chosen active perk with this key
        var slot = _perkController.perkSlots.FirstOrDefault(s => s != null
                                                                 && s.perk != null
                                                                 && s.chosen
                                                                 && s.mode == PerkMode.Active
                                                                 && s.activationKey == key);

        if (slot == null)
        {
            ClearBar(iconImage, timeText, bar);
            return;
        }

        // show icon and time/bar
        iconImage.enabled = true;
        timeText.enabled = true;
        bar.enabled = true;

        // assign perk icon sprite (hide if none)
        if (slot.perk.icon != null)
        {
            iconImage.sprite = slot.perk.icon;
            iconImage.color = Color.white;
        }
        else
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }

        float cd = Mathf.Max(0f, slot.perk.cooldown);
        float rem = Mathf.Max(0f, slot.cooldownRemaining);

        if (cd > 0f)
        {
            // fillAmount represents remaining fraction (1 = full cooldown, 0 = ready)
            bar.type = Image.Type.Filled;
            bar.fillMethod = Image.FillMethod.Horizontal;
            bar.fillOrigin = 0;
            bar.fillAmount = Mathf.Clamp01(rem / cd);

            if (rem > 0f)
                timeText.text = $"{Mathf.CeilToInt(rem)}s";
            else
                timeText.text = "Ready";
        }
        else
        {
            // no cooldown on this perk
            bar.type = Image.Type.Filled;
            bar.fillAmount = 0f;
            timeText.text = "Ready";
        }
    }

    private void ClearBar(Image iconImage, TextMeshProUGUI timeText, Image bar)
    {
        if (iconImage != null) { iconImage.sprite = null; iconImage.enabled = false; }
        if (timeText != null) { timeText.text = ""; timeText.enabled = false; }
        if (bar != null) { bar.fillAmount = 0f; bar.enabled = false; }
    }

    // existing methods
    public void SetHasKey(bool hasKey)
    {
        if (keyImage != null)
            keyImage.enabled = hasKey;
    }

    public void SetCrosshairVisible(bool visible)
    {
        if (crosshairPart1 != null)
            crosshairPart1.SetActive(visible);

        if (crosshairPart2 != null)
            crosshairPart2.SetActive(visible);
    }
}