using System.Collections;
using UnityEngine;

namespace AmesGame
{
    // Temporarily increases the player's movement speed
    public class SpeedyPerk : Perk
    {
        [Tooltip("Multiplier applied to player MoveSpeed while active")]
        public float speedMultiplier = 3f;

        [Tooltip("How long the speed boost lasts (seconds)")]
        public float duration = 5f;

        [Tooltip("Cooldown after using the perk (seconds)")]
        public float cooldown = 10f;

        private bool onCooldown = false;
        private PlayerController player;

        private void Awake()
        {
            player = GetComponentInParent<PlayerController>();
            if (player == null)
            {
                player = Object.FindAnyObjectByType<PlayerController>();
            }
        }

        public override void Activate()
        {
            if (onCooldown) return;

            if (player == null)
            {
                Debug.LogWarning("SpeedyPerk: No PlayerController found to apply speed boost.");
                return;
            }

            StartCoroutine(BoostRoutine());
        }

        private IEnumerator BoostRoutine()
        {
            onCooldown = true;

            float originalSpeed = player.MoveSpeed;
            player.MoveSpeed = originalSpeed * speedMultiplier;
            Debug.Log($"SpeedyPerk: Activated. MoveSpeed {originalSpeed} -> {player.MoveSpeed} for {duration}s.");

            yield return new WaitForSeconds(duration);

            // Restore
            if (player != null)
            {
                player.MoveSpeed = originalSpeed;
                Debug.Log($"SpeedyPerk: Deactivated. MoveSpeed restored to {player.MoveSpeed}.");
            }

            // cooldown
            yield return new WaitForSeconds(cooldown);
            onCooldown = false;
        }
    }
}
