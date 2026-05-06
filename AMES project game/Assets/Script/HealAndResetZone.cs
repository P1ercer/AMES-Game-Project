using UnityEngine;

namespace AmesGame
{
    public class HealAndResetZone : MonoBehaviour
    {
        [Header("Settings")]
        public bool destroyAfterUse = false;

        private void OnTriggerEnter(Collider other)
        {
            // Check if the object entering is the player
            PlayerController player = other.GetComponent<PlayerController>();
            if (player == null) return;

            // Heal to max
            player.Heal(player.MaxHealth);
            // Reset perks
            PerkController perkController = player.GetComponent<PerkController>();
            if (perkController != null)
            {
                ResetAllPerks(perkController);
            }

            Debug.Log("Player healed to max and perks reset!");

            // Optional: destroy the object after use
            if (destroyAfterUse)
            {
                Destroy(gameObject);
            }
        }

        private void ResetAllPerks(PerkController perkController)
        {
            foreach (var slot in perkController.perkSlots)
            {
                if (slot != null && slot.perk != null && slot.chosen)
                {
                    slot.perk.OnUnequip();
                    slot.chosen = false;
                }
            }
        }
    }
}