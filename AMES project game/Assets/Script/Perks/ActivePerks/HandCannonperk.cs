using System;
using System.Collections;
using UnityEngine;

namespace AmesGame
{
    // HandCannonperk: fires a projectile and knocks the player back (recoil) when used.
    public class HandCannonperk : Perk
    {
        // event fired when the hand cannon fires a projectile
        public event Action<GameObject> OnFired;
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
                player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
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

            // notify listeners that we've fired (pass spawned projectile)
            OnFired?.Invoke(proj);

            // Spawn directional VFX depending on player movement
            if (player != null)
            {
                var inputsForVfx = player.GetComponent<AmesGameInputs>();
                Vector2 moveForVfx = inputsForVfx != null ? inputsForVfx.move : Vector2.zero;

                float absX = Mathf.Abs(moveForVfx.x);
                float absY = Mathf.Abs(moveForVfx.y);

                // Visuals are handled by the projectile prefab or external systems. No animation is played here.
            }

            Debug.Log($"HandCannonperk: Fired projectile dealing {damage} damage.");

            // Apply recoil/knockback to player (prefer physics via Rigidbody, otherwise use PlayerController API)
            if (player != null)
            {
                Vector3 pushDir = -spawnPoint.forward;

                // Determine movement-based multiplier: standing still -> stronger recoil; moving -> still receives full recoil
                float multiplier = 1f;
                var inputs = player.GetComponent<AmesGameInputs>();
                Vector2 move = inputs != null ? inputs.move : Vector2.zero;

                if (move.magnitude < 0.1f)
                {
                    // standing still -> stronger recoil
                    multiplier = stationaryMultiplier;
                }

                Vector3 horizontalImpulse = pushDir.normalized * knockbackAmount * multiplier;
                float verticalImpulse = knockbackAmount * 0.25f; // small upward component

                // prefer Rigidbody-based physics
                var rbPlayer = player.GetComponent<Rigidbody>();
                if (rbPlayer != null)
                {
                    Vector3 total = horizontalImpulse + Vector3.up * verticalImpulse;
                    rbPlayer.AddForce(total, ForceMode.VelocityChange);
                }
                else
                {
                    // use PlayerController's ApplyKnockback if available
                    player.ApplyKnockback(horizontalImpulse, verticalImpulse);
                }

                Debug.Log($"HandCannonperk: Applied recoil to player. base={knockbackAmount}, multiplier={multiplier} => total={horizontalImpulse.magnitude}");

                // cooldown wait
                yield return new WaitForSeconds(cooldown);
                onCooldown = false;
            }
        }
    }
}