using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AmesGame
{
    public class PerkChooserUI : MonoBehaviour
    {
        public PerkController perkController;

        [Header("Roll Button")]
        public Button rollButton;

        [Header("All 30 Buttons (match perkSlots order)")]
        public List<Button> allButtons = new List<Button>();

        private List<PerkController.PerkSlot> availableSlots = new List<PerkController.PerkSlot>();
        private bool isChoosing = false;

        private void Start()
        {
            HideAll();

            // Hide UI + roll button at start
            gameObject.SetActive(false);
            if (rollButton != null)
                rollButton.gameObject.SetActive(false);

            if (rollButton != null)
                rollButton.onClick.AddListener(RollPerks);
        }

        // Called by enemy death
        public void ShowPerkUI()
        {
            if (isChoosing) return;

            gameObject.SetActive(true);

            // Show roll button
            if (rollButton != null)
                rollButton.gameObject.SetActive(true);

            // Pause game
            Time.timeScale = 0f;

            // Unlock cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            isChoosing = true;
        }

        void RollPerks()
        {
            availableSlots.Clear();

            // Get available perks
            for (int i = 0; i < perkController.perkSlots.Count; i++)
            {
                var slot = perkController.perkSlots[i];
                if (slot == null || slot.perk == null) continue;
                if (slot.chosen) continue;

                availableSlots.Add(slot);
            }

            if (availableSlots.Count < 3)
            {
                Debug.Log("Not enough perks left!");
                return;
            }

            HideAll();

            List<PerkController.PerkSlot> chosen = new List<PerkController.PerkSlot>();

            // Pick 3 unique
            while (chosen.Count < 3)
            {
                int rand = Random.Range(1, availableSlots.Count + 1);
                var pick = availableSlots[rand - 1];

                if (!chosen.Contains(pick))
                    chosen.Add(pick);
            }

            // Show correct buttons
            foreach (var slot in chosen)
            {
                int index = perkController.perkSlots.IndexOf(slot);
                if (index < 0 || index >= allButtons.Count) continue;

                Button btn = allButtons[index];
                btn.gameObject.SetActive(true);

                Text txt = btn.GetComponentInChildren<Text>();
                if (txt != null)
                    txt.text = slot.perk.name;

                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnPerkSelected(slot, index));
            }

            // Optional: hide roll button after rolling once
            rollButton.gameObject.SetActive(false);
        }

        void OnPerkSelected(PerkController.PerkSlot slot, int index)
        {
            // Add perk
            perkController.AddPerk(slot.perk);

            // Remove permanently
            perkController.perkSlots.Remove(slot);

            if (slot.perk != null)
                Destroy(slot.perk.gameObject);

            // Hide UI
            HideAll();
            gameObject.SetActive(false);

            // Hide roll button again
            if (rollButton != null)
                rollButton.gameObject.SetActive(false);

            // Resume game
            Time.timeScale = 1f;

            // Lock cursor back (FPS-style)
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            isChoosing = false;
        }

        void HideAll()
        {
            foreach (var btn in allButtons)
                btn.gameObject.SetActive(false);
        }
    }
}