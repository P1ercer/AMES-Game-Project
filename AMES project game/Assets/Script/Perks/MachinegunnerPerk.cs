using System.Collections;
using UnityEngine;

namespace AmesGame
{
    public class MachinegunnerPerk : Perk
    {
        [Tooltip("How much faster you shoot (2 = twice as fast)")]
        public float fireSpeedMultiplier = 3f;

        [Tooltip("Duration of the effect in seconds")]
        public float duration = 6f;

        [Tooltip("Cooldown after using the perk")]
        public float cooldown = 12f;

        private bool onCooldown = false;

        [SerializeField] private RaycastShoot shooter;

        private void Awake()
        {
            if (shooter == null)
                shooter = GetComponentInParent<RaycastShoot>();
        }

        public override void Activate()
        {
            if (onCooldown || shooter == null) return;

            StartCoroutine(ActivateRoutine());
        }

        private IEnumerator ActivateRoutine()
        {
            onCooldown = true;

            // 🔥 Convert "faster fire" into "lower cooldown"
            float cooldownMultiplier = 1f / fireSpeedMultiplier;

            shooter.AddCooldownMultiplier(cooldownMultiplier, duration);

            Debug.Log($"Machinegunner: {fireSpeedMultiplier}x fire speed for {duration}s");

            yield return new WaitForSeconds(duration);

            Debug.Log("Machinegunner: Effect ended");

            yield return new WaitForSeconds(cooldown);
            onCooldown = false;
        }
    }
}