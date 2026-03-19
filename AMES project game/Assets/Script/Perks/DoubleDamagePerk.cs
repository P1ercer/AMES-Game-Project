using System.Collections;
using UnityEngine;

namespace AmesGame
{
    public class DoubleDamagePerk : Perk
    {
        [Tooltip("Duration of double damage in seconds")]
        public float duration = 20f;

        [Tooltip("Cooldown after the effect ends")]
        public float cooldown = 30f;

        private bool onCooldown = false;

        public override void Activate()
        {
            if (onCooldown) return;

            StartCoroutine(ActivateRoutine());
        }

        private IEnumerator ActivateRoutine()
        {
            onCooldown = true;

            // Apply global multiplier
            EnemyController.DamageMultiplier *= 2f;
            float mult = EnemyController.DamageMultiplier;
            int example = Mathf.Max(1, Mathf.RoundToInt(1f * mult));
            Debug.Log($"DoubleDamagePerk: Activated. Damage multiplier is now {mult}x — a 1-damage hit will deal {example} for {duration} seconds.");

            yield return new WaitForSeconds(duration);

            // Revert multiplier
            EnemyController.DamageMultiplier /= 2f;
            float after = EnemyController.DamageMultiplier;
            Debug.Log($"DoubleDamagePerk: Deactivated. Damage multiplier reverted to {after}x.");

            // start cooldown
            yield return new WaitForSeconds(cooldown);

            onCooldown = false;
        }
    }
}
