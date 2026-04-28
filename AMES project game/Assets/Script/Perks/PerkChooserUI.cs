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
        public MonoBehaviour playerLook; // Drag your mouse look / camera script here

        [Header("UI")]
        public PlayerUI playerUI; // Reference to crosshair handler

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

        private void Start()
        {
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

        public void ShowPerkUI()
        {
            if (isChoosing) return;

            gameObject.SetActive(true);

            // ensure individual perk widgets are hidden until the player rolls
            HideAll();

            // if TheCost or APrice are active alone, do not allow rolling for new perks
            bool costActive = false;
            bool aPriceActive = false;
            if (perkController != null)
            {
                foreach (var s in perkController.perkSlots)
                {
                    if (s == null || s.perk == null) continue;
                    if (s.chosen && s.perk is TheCostPerk) costActive = true;
                    if (s.chosen && s.perk is APricePerk) aPriceActive = true;
                    if (costActive && aPriceActive) break;
                }
            }

            bool blockedSinglePerk = costActive ^ aPriceActive; // true if exactly one is active

            if (rollButton != null)
            {
                // If exactly one of TheCost or APrice is active, only sometimes allow rolling
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
            {
                // always show a close option when the perk UI opens
                closeRollButton.gameObject.SetActive(true);
            }

            Time.timeScale = 0f;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (playerShooter != null)
                playerShooter.canShoot = false;

            if (playerLook != null)
                playerLook.enabled = false;

            if (playerUI != null)
                playerUI.SetCrosshairVisible(false);

            isChoosing = true;
        }

        void RollPerks()
        {
            if (rollButton != null)
                rollButton.gameObject.SetActive(false);

            // Prevent rolling if TheCost or APrice is active alone
            bool costActive = false;
            bool aPriceActive = false;
            if (perkController != null)
            {
                foreach (var s in perkController.perkSlots)
                {
                    if (s == null || s.perk == null) continue;
                    if (s.chosen && s.perk is TheCostPerk) costActive = true;
                    if (s.chosen && s.perk is APricePerk) aPriceActive = true;
                    if (costActive && aPriceActive) break;
                }
            }
            bool rareMode = false;
            bool allowRareRoll = true;

            if (costActive ^ aPriceActive)
            {
                // Only allow the rare-mode roll if the UI previously allowed it when opened
                if (!rareRollAllowed) return;

                // rare mode: only give the counterpart perk
                rareMode = true;
                availableSlots.Clear();
                if (perkController != null)
                {
                    foreach (var slot in perkController.perkSlots)
                    {
                        if (slot == null || slot.perk == null) continue;
                        if (slot.chosen) continue;

                        if (costActive && slot.perk is APricePerk)
                            availableSlots.Add(slot);
                        if (aPriceActive && slot.perk is TheCostPerk)
                            availableSlots.Add(slot);
                    }
                }
            }
            else
            {
                availableSlots.Clear();

                // detect if HandCannonperk or EXPLOSION are currently equipped/chosen
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
                    if (handCannonEquipped && explosionEquipped && slamEquipped) break;
                }

                foreach (var slot in perkController.perkSlots)
                {
                    if (slot == null || slot.perk == null) continue;
                    if (slot.chosen) continue;

                    // Only show the Handcannon upgrade when handcannon is equipped
                    if (slot.perk is HandcannonUpgradePerk && !handCannonEquipped)
                        continue;

                    // Only show the Explosion upgrade when explosion is equipped
                    if (slot.perk is ExplosionUpgradePerk && !explosionEquipped)
                        continue;

                    // Only show the Slam upgrade when SlamDunk is equipped
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

                // populate UI from chosen
                for (int i = 0; i < choiceButtons.Count; i++)
                {
                    if (i >= chosen.Count) break;

                    var entry = choiceButtons[i];
                    var slot = chosen[i];
                    var perk = slot.perk;

                    // show the button and its child UI elements
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

            // rareMode: only offer the counterpart(s) available
            if (availableSlots.Count == 0)
                return;

            List<PerkController.PerkSlot> rareChosen = new List<PerkController.PerkSlot>();
            // add up to one copy of each available slot (avoid duplicates)
            foreach (var s in availableSlots)
            {
                if (!rareChosen.Contains(s)) rareChosen.Add(s);
                if (rareChosen.Count >= choiceButtons.Count) break;
            }

            for (int i = 0; i < choiceButtons.Count; i++)
            {
                if (i >= rareChosen.Count) break;

                var entry = choiceButtons[i];
                var slot = rareChosen[i];
                var perk = slot.perk;

                // show the button and its child UI elements
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
            perkController.AddPerk(slot.perk);

            HideAll();
            gameObject.SetActive(false);

            if (rollButton != null)
                rollButton.gameObject.SetActive(false);
            if (closeRollButton != null)
                closeRollButton.gameObject.SetActive(false);

            Time.timeScale = 1f;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (playerShooter != null)
                playerShooter.canShoot = true;

            if (playerLook != null)
                playerLook.enabled = true;

            if (playerUI != null)
                playerUI.SetCrosshairVisible(true);

            isChoosing = false;
        }

        void DisplayEquippedPerks()
        {
            HideAll();

            List<PerkController.PerkSlot> active = new List<PerkController.PerkSlot>();

            foreach (var slot in perkController.perkSlots)
            {
                if (slot != null && slot.perk != null && slot.chosen)
                    active.Add(slot);
            }

            for (int i = 0; i < choiceButtons.Count; i++)
            {
                if (i >= active.Count) break;

                var entry = choiceButtons[i];
                var perk = active[i].perk;

                entry.button.gameObject.SetActive(true);
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

                var slot = active[i];
                entry.button.onClick.RemoveAllListeners();
                entry.button.onClick.AddListener(() => RemovePerk(slot));
            }
        }

        void RemovePerk(PerkController.PerkSlot slot)
        {
            perkController.RemovePerk(slot.perk);
            DisplayEquippedPerks();
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
            gameObject.SetActive(false);

            // ▶ Resume game
            Time.timeScale = 1f;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (playerShooter != null)
                playerShooter.canShoot = true;

            if (playerLook != null)
                playerLook.enabled = true;

            if (playerUI != null)
                playerUI.SetCrosshairVisible(true);

            isChoosing = false;
        }
    }
}