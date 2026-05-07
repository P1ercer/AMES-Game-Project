using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AmesGame
{
    public class PerkChooserUI : MonoBehaviour
    {
        public PerkController perkController;

        [Header("Player")]
        public RaycastShoot playerShooter;

        [Header("Player Control")]
        public MonoBehaviour playerLook;

        [Header("UI")]
        public PlayerUI playerUI;

        [Header("Roll Button")]
        public Button rollButton;

        [Header("Close Roll Button")]
        public Button closeRollButton;

        [Header("Rare Roll Chance")]
        [Range(0f, 1f)]
        public float rareRollChance = 0.01f;

        [System.Serializable]
        public class PerkButton
        {
            public Button button;
            public TMP_Text label;
            public TMP_Text descriptionText;
            public Image iconImage;
        }

        public List<PerkButton> choiceButtons = new List<PerkButton>();

        private List<PerkController.PerkSlot> availableSlots = new List<PerkController.PerkSlot>();
        private bool isChoosing = false;
        private bool rareRollAllowed = false;

        [Header("Audio")]
        [Tooltip("Sound played when rolling perks")]
        public AudioClip rollSound;
        [Tooltip("Sound played when selecting a perk")]
        public AudioClip selectSound;
        [Tooltip("Sound played when closing the chooser")]
        public AudioClip closeSound;
        private AudioSource uiAudioSource;

        private void Start()
        {
            // Audio setup
            uiAudioSource = GetComponent<AudioSource>();
            if (uiAudioSource == null)
            {
                uiAudioSource = gameObject.AddComponent<AudioSource>();
                uiAudioSource.playOnAwake = false;
            }

            HideAll();
            gameObject.SetActive(false);

            if (rollButton != null)
            {
                rollButton.gameObject.SetActive(false);
                rollButton.onClick.AddListener(RollPerks);
            }

            if (closeRollButton != null)
            {
                closeRollButton.gameObject.SetActive(false);
                closeRollButton.onClick.AddListener(CloseUI);
            }
        }

        bool IsKeyAvailable(PerkController.PerkSlot slot)
        {
            if (slot.mode != PerkMode.Active)
                return true;

            foreach (var s in perkController.perkSlots)
            {
                if (s == null || s.perk == null) continue;

                if (s.chosen &&
                    s.mode == PerkMode.Active &&
                    s.activationKey == slot.activationKey)
                {
                    return false;
                }
            }

            return true;
        }

        public void ShowPerkUI()
        {
            if (isChoosing) return;

            gameObject.SetActive(true);
            HideAll();

            bool costActive = false;
            bool aPriceActive = false;

            foreach (var s in perkController.perkSlots)
            {
                if (s == null || s.perk == null) continue;

                if (s.chosen && s.perk is TheCostPerk) costActive = true;
                if (s.chosen && s.perk is APricePerk) aPriceActive = true;

                if (costActive && aPriceActive) break;
            }

            bool blockedSinglePerk = costActive ^ aPriceActive;

            if (rollButton != null)
            {
                if (blockedSinglePerk)
                {
                    rareRollAllowed = Random.value <= rareRollChance;
                    rollButton.gameObject.SetActive(rareRollAllowed);
                }
                else
                {
                    rareRollAllowed = false;
                    rollButton.gameObject.SetActive(true);
                }
            }

            if (closeRollButton != null)
                closeRollButton.gameObject.SetActive(true);

            // DO NOT pause game time here to keep UI input (clicks) working.
            // Only disable player movement and look so the player can't move while choosing.
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (playerShooter != null)
                playerShooter.canShoot = false;

            if (playerLook != null)
                playerLook.enabled = false;

            if (playerUI != null)
                playerUI.SetCrosshairVisible(false);

            // Pause enemy movement/shooting while chooser is open
            EnemyController.AddUiPause();

            isChoosing = true;
        }

        void RollPerks()
        {
            if (rollButton != null)
                rollButton.gameObject.SetActive(false);

            // play roll sound
            if (rollSound != null)
            {
                if (uiAudioSource != null)
                    uiAudioSource.PlayOneShot(rollSound);
                else
                    AudioSource.PlayClipAtPoint(rollSound, transform.position);
            }

            bool costActive = false;
            bool aPriceActive = false;

            foreach (var s in perkController.perkSlots)
            {
                if (s == null || s.perk == null) continue;

                if (s.chosen && s.perk is TheCostPerk) costActive = true;
                if (s.chosen && s.perk is APricePerk) aPriceActive = true;

                if (costActive && aPriceActive) break;
            }

            bool rareMode = false;

            if (costActive ^ aPriceActive)
            {
                if (!rareRollAllowed) return;

                rareMode = true;
                availableSlots.Clear();

                foreach (var slot in perkController.perkSlots)
                {
                    if (slot == null || slot.perk == null) continue;
                    if (slot.chosen) continue;
                    if (!IsKeyAvailable(slot)) continue;

                    if (costActive && slot.perk is APricePerk)
                        availableSlots.Add(slot);

                    if (aPriceActive && slot.perk is TheCostPerk)
                        availableSlots.Add(slot);
                }
            }
            else
            {
                availableSlots.Clear();

                bool handCannonEquipped = false;
                bool explosionEquipped = false;
                bool slamEquipped = false;

                foreach (var s in perkController.perkSlots)
                {
                    if (s == null || s.perk == null) continue;

                    if (s.chosen && s.perk is HandCannonperk)
                        handCannonEquipped = true;

                    if (s.chosen && s.perk is EXPLOSION)
                        explosionEquipped = true;

                    if (s.chosen && s.perk is SlamDunkPerk)
                        slamEquipped = true;

                    if (handCannonEquipped && explosionEquipped && slamEquipped)
                        break;
                }

                foreach (var slot in perkController.perkSlots)
                {
                    if (slot == null || slot.perk == null) continue;
                    if (slot.chosen) continue;
                    if (!slot.includeInRandom) continue;

                    // 🚫 NEW: keybind filtering
                    if (!IsKeyAvailable(slot))
                        continue;

                    if (slot.perk is HandcannonUpgradePerk && !handCannonEquipped)
                        continue;

                    if (slot.perk is ExplosionUpgradePerk && !explosionEquipped)
                        continue;

                    if (slot.perk is SlamUpgradePerk && !slamEquipped)
                        continue;

                    availableSlots.Add(slot);
                }
            }

            if (!rareMode)
            {
                if (availableSlots.Count < 3)
                    return;

                List<PerkController.PerkSlot> chosen = new List<PerkController.PerkSlot>();

                while (chosen.Count < 3)
                {
                    int rand = Random.Range(0, availableSlots.Count);
                    var pick = availableSlots[rand];

                    if (!chosen.Contains(pick))
                        chosen.Add(pick);
                }

                for (int i = 0; i < choiceButtons.Count; i++)
                {
                    if (i >= chosen.Count) break;

                    var entry = choiceButtons[i];
                    var slot = chosen[i];
                    var perk = slot.perk;

                    if (entry.button != null) entry.button.gameObject.SetActive(true);

                    if (entry.label != null)
                    {
                        entry.label.gameObject.SetActive(true);
                        entry.label.text = perk.perkName;
                    }

                    if (entry.descriptionText != null)
                    {
                        entry.descriptionText.gameObject.SetActive(true);
                        entry.descriptionText.text = perk.description;
                    }

                    if (entry.iconImage != null)
                    {
                        entry.iconImage.gameObject.SetActive(perk.icon != null);
                        entry.iconImage.sprite = perk.icon;
                        entry.iconImage.enabled = perk.icon != null;
                    }

                    entry.button.onClick.RemoveAllListeners();
                    entry.button.onClick.AddListener(() => OnPerkSelected(slot));
                }

                return;
            }

            if (availableSlots.Count == 0)
                return;

            List<PerkController.PerkSlot> rareChosen = new List<PerkController.PerkSlot>();

            foreach (var s in availableSlots)
            {
                if (!rareChosen.Contains(s))
                    rareChosen.Add(s);

                if (rareChosen.Count >= choiceButtons.Count)
                    break;
            }

            for (int i = 0; i < choiceButtons.Count; i++)
            {
                if (i >= rareChosen.Count) break;

                var entry = choiceButtons[i];
                var slot = rareChosen[i];
                var perk = slot.perk;

                if (entry.button != null) entry.button.gameObject.SetActive(true);

                if (entry.label != null)
                {
                    entry.label.gameObject.SetActive(true);
                    entry.label.text = perk.perkName;
                }

                if (entry.descriptionText != null)
                {
                    entry.descriptionText.gameObject.SetActive(true);
                    entry.descriptionText.text = perk.description;
                }

                if (entry.iconImage != null)
                {
                    entry.iconImage.gameObject.SetActive(perk.icon != null);
                    entry.iconImage.sprite = perk.icon;
                    entry.iconImage.enabled = perk.icon != null;
                }

                entry.button.onClick.RemoveAllListeners();
                entry.button.onClick.AddListener(() => OnPerkSelected(slot));
            }
        }

        void OnPerkSelected(PerkController.PerkSlot slot)
        {
            // play select sound
            if (selectSound != null)
            {
                if (uiAudioSource != null)
                    uiAudioSource.PlayOneShot(selectSound);
                else
                    AudioSource.PlayClipAtPoint(selectSound, transform.position);
            }

            perkController.AddPerk(slot.perk);

            HideAll();
            gameObject.SetActive(false);

            if (rollButton != null)
                rollButton.gameObject.SetActive(false);

            if (closeRollButton != null)
                closeRollButton.gameObject.SetActive(false);

            // restore player control without changing Time.timeScale
            if (playerShooter != null)
                playerShooter.canShoot = true;

            if (playerLook != null)
                playerLook.enabled = true;

            if (playerUI != null)
                playerUI.SetCrosshairVisible(true);

            // restore enemy movement/shooting
            EnemyController.RemoveUiPause();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            isChoosing = false;
        }

        void HideAll()
        {
            foreach (var entry in choiceButtons)
            {
                if (entry.button != null)
                    entry.button.gameObject.SetActive(false);

                if (entry.label != null)
                    entry.label.gameObject.SetActive(false);

                if (entry.descriptionText != null)
                    entry.descriptionText.gameObject.SetActive(false);

                if (entry.iconImage != null)
                    entry.iconImage.gameObject.SetActive(false);
            }
        }

        public void CloseUI()
        {
            // play close sound
            if (closeSound != null)
            {
                if (uiAudioSource != null)
                    uiAudioSource.PlayOneShot(closeSound);
                else
                    AudioSource.PlayClipAtPoint(closeSound, transform.position);
            }

            gameObject.SetActive(false);

            // restore player control without changing Time.timeScale
            if (playerShooter != null)
                playerShooter.canShoot = true;

            if (playerLook != null)
                playerLook.enabled = true;

            if (playerUI != null)
                playerUI.SetCrosshairVisible(true);

            // restore enemy movement/shooting
            EnemyController.RemoveUiPause();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            isChoosing = false;
        }
    }
}