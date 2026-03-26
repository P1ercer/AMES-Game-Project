using System.Collections;
using UnityEngine;

namespace AmesGame
{
    // Fires a projectile that, on hitting an enemy, applies damage over time (DoT).
    public class FireBaller : Perk
    {
        [Tooltip("Projectile prefab to spawn. Should have a Collider and Rigidbody")]
        public GameObject projectilePrefab;

        [Tooltip("Spawn point for the projectile")]
        public Transform spawnPoint;

        [Tooltip("Speed applied to the spawned projectile")]
        public float projectileSpeed = 18f;

        [Tooltip("How long before the spawned projectile is destroyed")]
        public float projectileLifetime = 6f;

        [Header("Damage over time")]
        [Tooltip("Damage applied per tick")]
        public int dotDamagePerTick = 1;

        [Tooltip("Seconds between ticks")]
        public float dotTickInterval = 1f;

        [Tooltip("Total duration of the DoT in seconds")]
        public float dotDuration = 6f;

        [Tooltip("Cooldown between uses")]
        public float cooldown = 2.5f;

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

            if (projectilePrefab == null || spawnPoint == null)
            {
                Debug.LogWarning("FireBaller: projectilePrefab or spawnPoint not assigned.");
                return;
            }

            StartCoroutine(FireRoutine());
        }

        private IEnumerator FireRoutine()
        {
            onCooldown = true;

            var proj = Instantiate(projectilePrefab, spawnPoint.position, spawnPoint.rotation);
            proj.tag = "PlayerBullet";

            // Attach helper to handle applying DoT on hit
            var fp = proj.GetComponent<FireProjectile>();
            if (fp == null) fp = proj.AddComponent<FireProjectile>();
            fp.dotDamage = dotDamagePerTick;
            fp.dotInterval = dotTickInterval;
            fp.dotDuration = dotDuration;

            // Launch
            var rb = proj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 aimDir = spawnPoint.forward.normalized;
                if (Camera.main != null)
                {
                    Vector3 aimPoint = Camera.main.transform.position + Camera.main.transform.forward * 100f;
                    aimDir = (aimPoint - spawnPoint.position).normalized;
                }

                rb.linearVelocity = aimDir * projectileSpeed;
            }

            Destroy(proj, projectileLifetime);

            Debug.Log($"FireBaller: Fired fire projectile (DoT {dotDamagePerTick} dmg/tick for {dotDuration}s).");

            yield return new WaitForSeconds(cooldown);
            onCooldown = false;
        }
    }

    // Helper component on the fired projectile
    public class FireProjectile : MonoBehaviour
    {
        [HideInInspector] public int dotDamage = 1;
        [HideInInspector] public float dotInterval = 1f;
        [HideInInspector] public float dotDuration = 5f;

        private void OnCollisionEnter(Collision collision)
        {
            TryApplyDot(collision.collider);
        }

        private void OnTriggerEnter(Collider other)
        {
            TryApplyDot(other);
        }

        private void TryApplyDot(Collider col)
        {
            if (col == null) return;

            var enemy = col.GetComponentInParent<EnemyController>();
            if (enemy == null) return;

            // Apply DoT component on the enemy gameobject
            var existing = enemy.GetComponent<FireDamageOverTime>();
            if (existing != null)
            {
                // refresh / stack: start a new instance as well
                // (optional: you could refresh duration instead)
            }

            var dot = enemy.gameObject.AddComponent<FireDamageOverTime>();
            dot.damagePerTick = dotDamage;
            dot.tickInterval = dotInterval;
            dot.duration = dotDuration;
            dot.StartDoT();

            Debug.Log($"FireProjectile: Applied DoT to '{enemy.gameObject.name}' ({dotDamage} dmg every {dotInterval}s for {dotDuration}s).");

            Destroy(gameObject);
        }
    }

    // Component that deals periodic damage to the attached enemy
    public class FireDamageOverTime : MonoBehaviour
    {
        public int damagePerTick = 1;
        public float tickInterval = 1f;
        public float duration = 5f;

        private EnemyController enemy;

        public void StartDoT()
        {
            enemy = GetComponent<EnemyController>();
            if (enemy == null)
            {
                // not an enemy, just remove
                Destroy(this);
                return;
            }

            StartCoroutine(DoTRoutine());
        }

        private IEnumerator DoTRoutine()
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (enemy == null) break;
                enemy.TakeDamage(damagePerTick);
                yield return new WaitForSeconds(tickInterval);
                elapsed += tickInterval;
            }

            Destroy(this);
        }
    }
}
