using UnityEngine;

namespace AmesGame
{
    public class MarioPerk : Perk
    {
        [Header("Jump Boost")]
        [Tooltip("Multiplier for jump height")]
        public float jumpMultiplier = 1.5f;

        [Header("Gravity Increase")]
        [Tooltip("Multiplier for gravity (higher = fall faster)")]
        public float gravityMultiplier = 1.4f;

        private PlayerController player;

        private float originalJumpHeight;
        private float originalGravity;

        public override void OnEquip()
        {
            base.OnEquip();

            player = GetComponent<PlayerController>();

            if (player != null)
            {
                // Store originals
                originalJumpHeight = player.JumpHeight;
                originalGravity = player.Gravity;

                // Apply effects
                player.JumpHeight *= jumpMultiplier;

                // Gravity is negative, so multiplying makes it MORE negative
                player.Gravity *= gravityMultiplier;
            }
        }

        public override void OnUnequip()
        {
            base.OnUnequip();

            if (player != null)
            {
                // Restore originals
                player.JumpHeight = originalJumpHeight;
                player.Gravity = originalGravity;
            }
        }
    }
}