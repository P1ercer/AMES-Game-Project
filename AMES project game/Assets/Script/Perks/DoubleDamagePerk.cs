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
            Debug.Log($"DoubleDamagePerk: Activated. Damage multiplier is now {EnemyController.DamageMultiplier} for {duration} seconds.");

            yield return new WaitForSeconds(duration);

            // Revert multiplier
            EnemyController.DamageMultiplier /= 2f;
            Debug.Log("DoubleDamagePerk: Deactivated.");

            // start cooldown
            yield return new WaitForSeconds(cooldown);

            onCooldown = false;
        }
    }
}
