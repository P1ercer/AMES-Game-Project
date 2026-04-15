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

        private void Start()
        {
            HideAll();
            gameObject.SetActive(false);

            if (rollButton != null)
            {
                rollButton.gameObject.SetActive(false);
                rollButton.onClick.AddListener(RollPerks);
            }
        }

        public void ShowPerkUI()
        {
            if (isChoosing) return;

            gameObject.SetActive(true);

            if (rollButton != null)
                rollButton.gameObject.SetActive(true);

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
            availableSlots.Clear();

            foreach (var slot in perkController.perkSlots)
            {
                if (slot == null || slot.perk == null) continue;
                if (slot.chosen) continue;

                availableSlots.Add(slot);
            }

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

                entry.button.gameObject.SetActive(true);

                entry.label.text = perk.perkName;
                entry.descriptionText.text = perk.description;

                if (entry.iconImage != null)
                {
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

                entry.label.text = perk.perkName;
                entry.descriptionText.text = perk.description;

                if (entry.iconImage != null)
                {
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