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

        [Header("Roll Button")]
        public Button rollButton;

        [System.Serializable]
        public class PerkButton
        {
            public Button button;
            public TMP_Text label;
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
            {
                Debug.Log("Not enough perks left!");
                return;
            }

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

                entry.button.gameObject.SetActive(true);

                string displayName = !string.IsNullOrEmpty(slot.perk.perkName)
                    ? slot.perk.perkName
                    : slot.perk.name;

                if (entry.label != null)
                    entry.label.text = displayName;

                entry.button.onClick.RemoveAllListeners();

                PerkController.PerkSlot capturedSlot = slot;
                entry.button.onClick.AddListener(() => OnPerkSelected(capturedSlot));
            }
        }

        void OnPerkSelected(PerkController.PerkSlot slot)
        {
            perkController.AddPerk(slot.perk);
            slot.chosen = true;

            HideAll();
            gameObject.SetActive(false);

            if (rollButton != null)
                rollButton.gameObject.SetActive(false);

            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (playerShooter != null)
                playerShooter.canShoot = true;

            isChoosing = false;
        }

        public void ShowCurrentPerks()
        {
            gameObject.SetActive(true);
            Time.timeScale = 0f;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (playerShooter != null)
                playerShooter.canShoot = false;

            DisplayEquippedPerks();
        }

        void DisplayEquippedPerks()
        {
            HideAll();

            List<PerkController.PerkSlot> activePerks = new List<PerkController.PerkSlot>();

            foreach (var slot in perkController.perkSlots)
            {
                if (slot == null || slot.perk == null) continue;
                if (slot.chosen)
                    activePerks.Add(slot);
            }

            for (int i = 0; i < choiceButtons.Count; i++)
            {
                if (i >= activePerks.Count) break;

                var entry = choiceButtons[i];
                var slot = activePerks[i];

                entry.button.gameObject.SetActive(true);

                string displayName = !string.IsNullOrEmpty(slot.perk.perkName)
                    ? slot.perk.perkName
                    : slot.perk.name;

                if (entry.label != null)
                    entry.label.text = displayName;

                entry.button.onClick.RemoveAllListeners();
                PerkController.PerkSlot capturedSlot = slot;
                entry.button.onClick.AddListener(() => RemovePerk(capturedSlot));
            }
        }

        void RemovePerk(PerkController.PerkSlot slot)
        {
            if (slot == null || slot.perk == null) return;

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
            Time.timeScale = 1f;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (playerShooter != null)
                playerShooter.canShoot = true;
        }
    }
}