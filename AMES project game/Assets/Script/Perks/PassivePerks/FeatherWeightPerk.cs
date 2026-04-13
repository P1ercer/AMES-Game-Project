using UnityEngine;

namespace AmesGame
{
    public class FeatherweightPerk : Perk
    {
        [Header("Gravity Reduction")]
        [Tooltip("Multiplier applied to gravity (0.5 = half gravity)")]
        public float gravityMultiplier = 0.6f;

        private PlayerController player;

        private float originalDeceleration;
        private float originalGravity;

        public override void OnEquip()
        {
            base.OnEquip();

            player = GetComponent<PlayerController>();

            if (player != null)
            {
                // Store originals
                originalDeceleration = player.DecelerationRate;
                originalGravity = player.Gravity;

                // remove slowdown
                player.DecelerationRate = 0f;

                // Reduce gravity
                player.Gravity *= gravityMultiplier;
            }
        }

        public override void OnUnequip()
        {
            base.OnUnequip();

            if (player != null)
            {
                // Restore values
                player.DecelerationRate = originalDeceleration;
                player.Gravity = originalGravity;
            }
        }
    }
}