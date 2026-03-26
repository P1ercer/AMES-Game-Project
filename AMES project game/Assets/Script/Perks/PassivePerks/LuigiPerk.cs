using UnityEngine;

namespace AmesGame
{
    public class LuigiPerk : Perk
    {
        [Header("Jump & Gravity")]
        public float jumpMultiplier = 1.4f;
        public float gravityMultiplier = 0.6f;

        [Header("Health Penalty")]
        public float healthMultiplier = 0.75f;

        [Header("Jump Cooldown Penalty")]
        public float jumpTimeoutMultiplier = 1.5f;

        [Header("Damage Penalty")]
        public float damageMultiplier = 0.7f;

        private PlayerController player;
        private RaycastShoot shooter;

        private float originalJumpHeight;
        private float originalGravity;
        private int originalMaxHealth;
        private float originalJumpTimeout;
        private int originalDamage;

        public override void OnEquip()
        {
            base.OnEquip();

            player = GetComponent<PlayerController>();
            shooter = GetComponentInChildren<RaycastShoot>();

            if (player != null)
            {
                // Store originals
                originalJumpHeight = player.JumpHeight;
                originalGravity = player.Gravity;
                originalMaxHealth = player.MaxHealth;
                originalJumpTimeout = player.JumpTimeout;

                // Apply movement changes
                player.JumpHeight *= jumpMultiplier;
                player.Gravity *= gravityMultiplier;

                // Health penalty
                player.MaxHealth = Mathf.RoundToInt(player.MaxHealth * healthMultiplier);
                player.CurrentHealth = Mathf.Min(player.CurrentHealth, player.MaxHealth);

                // Jump cooldown penalty
                player.JumpTimeout *= jumpTimeoutMultiplier;
            }

            if (shooter != null)
            {
                originalDamage = shooter.damage;
                shooter.damage = Mathf.RoundToInt(shooter.damage * damageMultiplier);
            }
        }

        public override void OnUnequip()
        {
            base.OnUnequip();

            if (player != null)
            {
                player.JumpHeight = originalJumpHeight;
                player.Gravity = originalGravity;
                player.MaxHealth = originalMaxHealth;
                player.JumpTimeout = originalJumpTimeout;

                player.CurrentHealth = Mathf.Clamp(player.CurrentHealth, 0, player.MaxHealth);
            }

            if (shooter != null)
            {
                shooter.damage = originalDamage;
            }
        }
    }
}