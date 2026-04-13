using UnityEngine;

namespace AmesGame
{
    public class TinyPerk : Perk
    {
        [Header("Tiny Settings")]
        public float speedMultiplier = 1.3f;   // +30% speed
        public float healthMultiplier = 0.7f;  // -30% max HP

        private PlayerController player;

        private float originalSpeed;
        private int originalMaxHealth;

        private bool applied = false;

        private void Awake()
        {
            perkName = "Tiny";
        }

        public override void OnEquip()
        {
            base.OnEquip();

            player = GetComponent<PlayerController>();

            if (player == null)
            {
                Debug.LogError("TinyPerk: No PlayerController found!");
                return;
            }

            if (applied) return;

            // Store original values
            originalSpeed = player.MoveSpeed;
            originalMaxHealth = player.MaxHealth;

            // Apply modifiers
            player.MoveSpeed *= speedMultiplier;

            player.MaxHealth = Mathf.RoundToInt(player.MaxHealth * healthMultiplier);
            player.CurrentHealth = Mathf.Clamp(player.CurrentHealth, 0, player.MaxHealth);

            applied = true;

            Debug.Log("Tiny perk applied: Faster but weaker.");
        }

        public override void OnUnequip()
        {
            base.OnUnequip();

            if (player == null || !applied) return;

            // Restore original values
            player.MoveSpeed = originalSpeed;
            player.MaxHealth = originalMaxHealth;

            player.CurrentHealth = Mathf.Clamp(player.CurrentHealth, 0, player.MaxHealth);

            applied = false;

            Debug.Log("Tiny perk removed.");
        }
    }
}