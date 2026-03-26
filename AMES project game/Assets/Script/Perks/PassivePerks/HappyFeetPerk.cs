using UnityEngine;

namespace AmesGame
{
    public class HappyFeetPerk : Perk
    {
        [Header("Happy Feet Settings")]
        [Tooltip("Multiplier applied to jump height (reduces jump height).")]
        [Range(0f, 1f)]
        public float jumpHeightMultiplier = 0.5f;

        private PlayerController player;
        private float originalJumpHeight;

        private void Awake()
        {
            player = GetComponent<PlayerController>();
        }

        public override void OnEquip()
        {
            base.OnEquip();

            if (player != null)
            {
                // Store original jump height
                originalJumpHeight = player.JumpHeight;

                // Reduce jump height
                player.JumpHeight *= jumpHeightMultiplier;

                // Force continuous jumping
                player.StartCoroutine(ContinuousJump());
            }
        }

        public override void OnUnequip()
        {
            base.OnUnequip();

            if (player != null)
            {
                // Restore original jump height
                player.JumpHeight = originalJumpHeight;

                // Stop the continuous jump coroutine
                player.StopCoroutine(ContinuousJump());
            }
        }

        public System.Collections.IEnumerator ContinuousJump()
        {
            while (true)
            {
                if (player.Grounded)
                {
                    player._verticalVelocity = Mathf.Sqrt(player.JumpHeight * -2f * player.Gravity);
                }
                yield return null;
            }
        }
    }
}