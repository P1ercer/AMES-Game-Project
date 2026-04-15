using System.Collections;
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

        [Tooltip("Force applied to nearby enemies when exploded")]
        public float explosionKnockback = 12f;
        [Tooltip("Global multiplier applied to explosion knockback to increase strength")]
        public float knockbackMultiplier = 3f;

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
            expl.knockback = explosionKnockback;
            expl.knockbackMultiplier = knockbackMultiplier;
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
        [HideInInspector] public float knockback = 8f;
        [HideInInspector] public float knockbackMultiplier = 1.0f;
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
            foreach (var c in hits)
            {
                if (c == null) continue;
                var enemy = c.GetComponentInParent<EnemyController>();
                if (enemy == null) continue;

                // Distinguish directly hit collider vs nearby ones
                if (directHit != null && c == directHit)
                {
                    enemy.TakeDamage(damage);
                    hitCount++;

                    Rigidbody rb = c.attachedRigidbody ?? c.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        Vector3 dir = (c.transform.position - pos).normalized;
                        if (dir.sqrMagnitude < 0.01f) dir = Vector3.up;
                        rb.AddForce(dir * knockback * knockbackMultiplier, ForceMode.Impulse);
                    }
                    else
                    {
                        var agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
                        if (agent != null && agent.isOnNavMesh)
                        {
                            Vector3 away = (enemy.transform.position - pos).normalized;
                            if (away.sqrMagnitude < 0.01f) away = Vector3.back;
                            agent.SetDestination(enemy.transform.position + away * 2f);
                        }
                    }
                }
                else
                {
                    // nearby: reduced knockback and damage
                    int reduced = Mathf.Max(1, Mathf.RoundToInt(damage * 0.6f));
                    enemy.TakeDamage(reduced);
                    hitCount++;

                    Rigidbody rb = c.attachedRigidbody ?? c.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        Vector3 dir = (c.transform.position - pos).normalized;
                        if (dir.sqrMagnitude < 0.01f) dir = Vector3.up;
                        rb.AddForce(dir * (knockback * 0.5f * knockbackMultiplier), ForceMode.Impulse);
                    }
                    else
                    {
                        var agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
                        if (agent != null && agent.isOnNavMesh)
                        {
                            Vector3 away = (enemy.transform.position - pos).normalized;
                            if (away.sqrMagnitude < 0.01f) away = Vector3.back;
                            agent.SetDestination(enemy.transform.position + away * 1f);
                        }
                    }
                }
            }

            Debug.Log($"ExplosionProjectile: Exploded at {pos} affecting {hitCount} enemies (direct hit: {directHit}).");

            Destroy(gameObject);
        }
    }
}
