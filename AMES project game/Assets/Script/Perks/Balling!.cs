using System.Collections;
using UnityEngine;

namespace AmesGame
{
    // This perk redirects projectiles that would hit the player.
    // When active, incoming enemy projectiles that collide with the player's collider
    // will be redirected away with reduced damage.
    public class BallingPerk : Perk
    {
        [Tooltip("Duration the redirection stays active")]
        public float duration = 10f;

        [Tooltip("Cooldown after the effect ends")]
        public float cooldown = 20f;

        [Tooltip("Multiplier applied to redirected projectile damage (0.5 = half damage)")]
        public float redirectedDamageMultiplier = 0.5f;

        private bool active = false;
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

            StartCoroutine(ActivateRoutine());
        }

        private IEnumerator ActivateRoutine()
        {
            active = true;
            onCooldown = true;

            Debug.Log($"BallingPerk: Activated for {duration} seconds.");

            // Give player temporary immunity while the perk is active
            if (player != null)
            {
                player.SetTemporaryImmunity(duration);
            }
            else
            {
                Debug.LogWarning("BallingPerk: No PlayerController found; cannot grant immunity.");
            }

            yield return new WaitForSeconds(duration);

            active = false;

            Debug.Log("BallingPerk: Deactivated.");

            yield return new WaitForSeconds(cooldown);

            onCooldown = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!active) return;

            // Only react to enemy projectiles
            if (!other.gameObject.CompareTag("EnemyBullet")) return;

            // Try to get a Rigidbody to reflect
            Rigidbody rb = other.attachedRigidbody ?? other.GetComponent<Rigidbody>();
            if (rb == null) return;

            // Try to find the nearest enemy to redirect the projectile to
            EnemyController[] enemies = Object.FindObjectsByType<EnemyController>(FindObjectsInactive.Include, FindObjectsSortMode.None); EnemyController nearest = null;
            float bestDist = float.MaxValue;
            Vector3 projPos = other.transform.position;

            foreach (var e in enemies)
            {
                if (e == null) continue;
                float d = Vector3.SqrMagnitude(e.transform.position - projPos);
                if (d < bestDist)
                {
                    bestDist = d;
                    nearest = e;
                }
            }

            // Determine redirect direction: toward nearest enemy if found, otherwise reflect away
            Vector3 dir;
            if (nearest != null)
            {
                dir = (nearest.transform.position - projPos).normalized;
            }
            else
            {
                dir = (projPos - transform.position).normalized;
                if (dir.sqrMagnitude < 0.01f) dir = transform.forward;
            }

            // Preserve current projectile speed if available
            float speed = rb.linearVelocity.magnitude;
            if (speed < 0.01f) speed = 10f;

            rb.linearVelocity = dir * speed;

            // If projectile has a ProjectileDamage component, reduce its damage and retag to PlayerBullet
            var pd = other.GetComponent<ProjectileDamage>() ?? other.gameObject.AddComponent<ProjectileDamage>();
            pd.damage = Mathf.Max(1, Mathf.RoundToInt(pd.damage * redirectedDamageMultiplier));

            other.gameObject.tag = "PlayerBullet";

            if (nearest != null)
            {
                Debug.Log($"BallingPerk: Redirected projectile to enemy '{nearest.gameObject.name}' with damage {pd.damage}.");
            }
            else
            {
                Debug.Log($"BallingPerk: Reflected projectile away from player with damage {pd.damage}.");
            }
        }
    }
}
