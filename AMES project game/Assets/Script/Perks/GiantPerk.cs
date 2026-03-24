using UnityEngine;

namespace AmesGame
{
    public class GiantPerk : Perk
    {
        [Header("Giant Settings")]
        public float speedMultiplier = 0.7f;   // -30% Speed
        public float healthMultiplier = 1.3f;  // +30% max HP

        private PlayerController player;

        private float originalSpeed;
        private int originalMaxHealth;

        private bool applied = false;

        private void Awake()
        {
            perkName = "Giant";
        }

        public override void OnEquip()
        {
            base.OnEquip();

            player = GetComponent<PlayerController>();

            if (player == null)
            {
                Debug.LogError("GiantPerk: No PlayerController found!");
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

            Debug.Log("Giant perk applied: Stronger but slower.");
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

            Debug.Log("Giant perk removed.");
        }
    }
}