using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AmesGame
{
    // Simple UI to choose a random perk from available PerkSlots and enable it
    public class PerkChooserUI : MonoBehaviour
    {
        public PerkController perkController;

        [Tooltip("Button that triggers random selection")]
        public Button randomButton;

        [Tooltip("Text field to show selected perk name")]
        public Text selectedPerkText;

        [Tooltip("Optional: Text to show current chosen perks count")]
        public Text activeCountText;

        private void Start()
        {
            if (randomButton != null)
                randomButton.onClick.AddListener(OnRandomButton);

            UpdateActiveCount();
        }

        private void OnRandomButton()
        {
            if (perkController == null) return;

            // pick random from available slots that are not already chosen and not null
            List<PerkController.PerkSlot> candidates = new List<PerkController.PerkSlot>();
            foreach (var slot in perkController.perkSlots)
            {
                if (slot == null || slot.perk == null) continue;
                if (slot.chosen) continue;
                candidates.Add(slot);
            }

            if (candidates.Count == 0)
            {
                if (selectedPerkText != null) selectedPerkText.text = "No available perks";
                return;
            }

            var choice = candidates[Random.Range(0, candidates.Count)];

            // request controller to add the perk (will respect maxPerks)
            perkController.AddPerk(choice.perk);

            if (choice.chosen)
            {
                if (selectedPerkText != null) selectedPerkText.text = "Already chosen: " + choice.perk.name;
            }
            else
            {
                if (selectedPerkText != null) selectedPerkText.text = "Selected: " + choice.perk.name;
            }

            UpdateActiveCount();
        }

        private void UpdateActiveCount()
        {
            if (activeCountText == null || perkController == null) return;
            int chosenCount = 0;
            foreach (var s in perkController.perkSlots)
                if (s != null && s.chosen) chosenCount++;

            activeCountText.text = $"Chosen: {chosenCount}/{perkController.MaxPerks}";
        }
    }
}
