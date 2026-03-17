using System.Collections;
using UnityEngine;

namespace AmesGame
{
    public class ForceFieldsPerk : Perk
    {
        [Tooltip("How long the forcefield lasts (seconds)")]
        public float duration = 15f;

        [Tooltip("Time between uses (seconds)")]
        public float cooldown = 30f;

        [Tooltip("Optional VFX to enable while forcefield is active")]
        public GameObject forcefieldVfx;

        private bool onCooldown = false;

        private PlayerController player;

        private void Awake()
        {
            // Try to find player in parents or scene
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
                Debug.LogWarning("ForceFieldsPerk: No PlayerController found to apply immunity.");
                return;
            }

            StartCoroutine(ActivateRoutine());
        }

        private IEnumerator ActivateRoutine()
        {
            onCooldown = true;

            // enable VFX if provided
            if (forcefieldVfx != null)
                forcefieldVfx.SetActive(true);

            // give player immunity
            player.SetTemporaryImmunity(duration);

            // debug log when forcefield becomes active
            Debug.Log($"ForceFieldsPerk: Activated on '{player.gameObject.name}' for {duration} seconds.");

            // wait duration
            yield return new WaitForSeconds(duration);

            if (forcefieldVfx != null)
                forcefieldVfx.SetActive(false);

            Debug.Log("ForceFieldsPerk: Deactivated.");

            // start cooldown timer
            yield return new WaitForSeconds(cooldown);

            onCooldown = false;
        }
    }
}
