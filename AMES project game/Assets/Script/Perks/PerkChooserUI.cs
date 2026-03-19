using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AmesGame
{
    // Simple UI to choose a random perk from available PerkSlots and enable it
    public class PerkChooserUI : MonoBehaviour
    {
        public PerkController perkController;

        // The random button was removed. Selection will be performed automatically or via the public API.

        [Tooltip("Text field to show selected perk name")]
        public Text selectedPerkText;

        [Tooltip("Optional: Text to show current active perks count")]
        public Text activeCountText;

        private void Start()
        {
            // Choose a random available perk on start
            UpdateActiveCount();
            ChooseRandomPerk();
        }

        // Public API to pick a random available perk from the player's perk slots
        public void ChooseRandomPerk()
        {
            if (perkController == null) return;

            // pick random from available slots that are not already active and not null
            // pick random from all player's perk slots that are not already active and not null
            List<PerkController.PerkSlot> candidates = new List<PerkController.PerkSlot>();
            foreach (var slot in perkController.perkSlots)
            {
                if (slot == null || slot.perk == null) continue;
                if (perkController.activePerks.Contains(slot.perk)) continue;
                candidates.Add(slot);
            }

            if (candidates.Count == 0)
            {
                if (selectedPerkText != null) selectedPerkText.text = "No available perks";
                return;
            }

            var choice = candidates[Random.Range(0, candidates.Count)];

            // mark chosen and add to active perks (respect maxPerks)
            choice.chosen = true;
            perkController.AddPerk(choice.perk);

            if (selectedPerkText != null) selectedPerkText.text = "Selected: " + choice.perk.name;

            UpdateActiveCount();
        }

        private void UpdateActiveCount()
        {
            if (activeCountText == null || perkController == null) return;
            activeCountText.text = $"Active: {perkController.activePerks.Count}/{perkController.MaxPerks}";
        }
    }
}
