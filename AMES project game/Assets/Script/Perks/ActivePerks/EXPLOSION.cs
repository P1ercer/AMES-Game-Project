using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AmesGame
{
    public class EXPLOSION : Perk
    {
        [Tooltip("Projectile prefab to spawn. Should have a Collider and Rigidbody (can be simple sphere)")]
        public GameObject projectilePrefab;

        [Tooltip("Spawn point for the projectile")]
        public Transform spawnPoint;

        [Tooltip("Speed applied to the spawned projectile")]
        public float projectileSpeed = 18f;

        [Tooltip("Lifetime of the projectile before it auto-explodes")]
        public float projectileLifetime = 5f;

        [Header("Explosion")]
        [Tooltip("Radius of the explosion")]
        public float explosionRadius = 5f;

        [Tooltip("Damage dealt to enemies in the explosion radius")]
        public int explosionDamage = 4;

        [Tooltip("Optional VFX prefab to spawn at explosion")]
        public GameObject explosionVfx;

        [Tooltip("Cooldown between uses")]
        public float cooldown = 3f;

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
                Debug.LogWarning("EXPLOSION: projectilePrefab or spawnPoint not assigned.");
                return;
            }

            StartCoroutine(FireRoutine());
        }

        private IEnumerator FireRoutine()
        {
            onCooldown = true;

            var proj = Instantiate(projectilePrefab, spawnPoint.position, spawnPoint.rotation);

            // Ensure projectile carries damage info (for other systems)
            var pd = proj.GetComponent<ProjectileDamage>() ?? proj.AddComponent<ProjectileDamage>();
            pd.damage = explosionDamage;

            proj.tag = "PlayerBullet";

            var expl = proj.GetComponent<ExplosionProjectile>();
            if (expl == null) expl = proj.AddComponent<ExplosionProjectile>();
            expl.radius = explosionRadius;
            expl.damage = explosionDamage;
            expl.vfx = explosionVfx;
            expl.owner = this;

            var rb = proj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 aimDir;
                if (Camera.main != null)
                {
                    Vector3 aimPoint = Camera.main.transform.position + Camera.main.transform.forward * 100f;
                    aimDir = (aimPoint - spawnPoint.position).normalized;
                }
                else
                {
                    aimDir = spawnPoint.forward.normalized;
                }

                rb.linearVelocity = aimDir * projectileSpeed;
            }

            Destroy(proj, projectileLifetime);

            Debug.Log($"EXPLOSION: Fired explosive projectile (damage {explosionDamage}, radius {explosionRadius}).");

            yield return new WaitForSeconds(cooldown);
            onCooldown = false;
        }
    }

    // Helper component placed on spawned projectile to handle exploding on impact
    public class ExplosionProjectile : MonoBehaviour
    {
        [HideInInspector] public float radius = 5f;
        [HideInInspector] public int damage = 4;
        [HideInInspector] public GameObject vfx;
        [HideInInspector] public EXPLOSION owner;

        private void OnCollisionEnter(Collision collision)
        {
            Explode(collision.collider);
        }

        private void OnTriggerEnter(Collider other)
        {
            // also explode on trigger hit
            Explode(other);
        }

        private void Explode(Collider directHit)
        {
            Vector3 pos = transform.position;

            if (vfx != null)
            {
                Instantiate(vfx, pos, Quaternion.identity);
            }

            // Find all potential targets
            Collider[] hits = Physics.OverlapSphere(pos, radius);

            int hitCount = 0;

            // Prevent hitting same enemy multiple times (if multiple colliders)
            HashSet<EnemyController> damagedEnemies = new HashSet<EnemyController>();

            foreach (var c in hits)
            {
                if (c == null) continue;

                var enemy = c.GetComponentInParent<EnemyController>();
                if (enemy == null) continue;

                if (damagedEnemies.Contains(enemy)) continue;

                enemy.TakeDamage(damage);
                damagedEnemies.Add(enemy);
                hitCount++;
            }

            Debug.Log($"ExplosionProjectile: Exploded at {pos} affecting {hitCount} enemies.");

            Destroy(gameObject);
        }
    }
}