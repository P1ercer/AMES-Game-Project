using System.Collections;
using UnityEngine;

namespace AmesGame
{
    // HandCannonperk: fires a projectile and knocks the player back (recoil) when used.
    public class HandCannonperk : Perk
    {
        [Tooltip("Projectile prefab to spawn (should have a Rigidbody)")]
        public GameObject projectilePrefab;

        [Tooltip("Spawn point for the projectile")]
        public Transform spawnPoint;

        [Tooltip("Speed applied to the spawned projectile")]
        public float projectileSpeed = 20f;

        [Tooltip("How long before the spawned projectile is destroyed")]
        public float projectileLifetime = 5f;

        [Tooltip("Damage the projectile deals")]
        public int damage = 5;

        [Tooltip("Amount of knockback applied to the player when firing")]
        public float knockbackAmount = 5f;

        [Tooltip("Multiplier applied to knockback when the player is standing still")]
        public float stationaryMultiplier = 3f;

        [Tooltip("Cooldown between shots")]
        public float cooldown = 1.2f;

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
                Debug.LogWarning("HandCannonperk: Missing projectilePrefab or spawnPoint.");
                return;
            }

            StartCoroutine(FireRoutine());
        }

        private IEnumerator FireRoutine()
        {
            onCooldown = true;

            // Spawn projectile
            var proj = Instantiate(projectilePrefab, spawnPoint.position, spawnPoint.rotation);

            // Set damage if ProjectileDamage component is used
            var pd = proj.GetComponent<ProjectileDamage>() ?? proj.AddComponent<ProjectileDamage>();
            pd.damage = damage;

            // Ensure tag is PlayerBullet so enemies will take damage
            proj.tag = "PlayerBullet";

            // Apply velocity
            var rb = proj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 dir = spawnPoint.forward;
                // preserve forward of spawn; apply speed
                rb.linearVelocity = dir.normalized * projectileSpeed;
            }

            Destroy(proj, projectileLifetime);

            Debug.Log($"HandCannonperk: Fired projectile dealing {damage} damage.");

            // Apply recoil/knockback to player
            if (player != null)
            {
                var cc = player.GetComponent<CharacterController>();
                Vector3 pushDir = -spawnPoint.forward;

                // Determine movement-based multiplier: standing still -> larger recoil; moving forward/sideways -> reduced recoil
                float multiplier = 1f;
                var inputs = player.GetComponent<AmesGameInputs>();
                Vector2 move = inputs != null ? inputs.move : Vector2.zero;

                if (move.magnitude < 0.1f)
                {
                    // standing still -> stronger recoil
                    multiplier = stationaryMultiplier;
                }
                else
                {
                    // moving forward reduces recoil more than strafing
                    if (move.y > 0.1f)
                        multiplier = 0.5f; // moving forward
                    else if (Mathf.Abs(move.x) > 0.1f)
                        multiplier = 0.7f; // strafing
                }

                Vector3 displacement = pushDir.normalized * knockbackAmount * multiplier;

                if (cc != null)
                {
                    // Move character controller immediately (small teleport-like impulse)
                    cc.Move(displacement);
                }
                else
                {
                    // fallback: move transform
                    player.transform.position += displacement;
                }

                Debug.Log($"HandCannonperk: Applied recoil to player. base={knockbackAmount}, multiplier={multiplier}, final={displacement.magnitude}.");
            }

            // cooldown wait
            yield return new WaitForSeconds(cooldown);
            onCooldown = false;
        }
    }
}
