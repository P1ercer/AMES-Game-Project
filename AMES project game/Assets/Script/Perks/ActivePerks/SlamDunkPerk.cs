using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace AmesGame
{
    // SlamDunkPerk: performs a slam centred on the player that deals AoE damage
    public class SlamDunkPerk : Perk
    {
        // invoked when the slam lands; passes center position and colliders hit
        public event Action<Vector3, Collider[]> OnSlamLanded;
        [Tooltip("Damage dealt to each enemy in the AoE")]
        public int damage = 3;

        [Tooltip("Radius of the AoE (meters)")]
        public float radius = 5f;



        [Tooltip("Cooldown after using the slam (seconds)")]
        public float cooldown = 8f;

        [Tooltip("Optional knockback force applied to rigidbody enemies")]
        public float knockbackForce = 6f;

        private bool onCooldown = false;

        [Tooltip("Vertical velocity to apply to player when slamming while midair (negative = down)")]
        public float airDropVelocity = -25f;
        [Tooltip("Multiplier applied to the air drop velocity to increase how fast the player is pushed down")]
        public float airDropStrength = 2f;
        [Tooltip("Multiplier to temporarily increase player gravity while forcing the drop")]
        public float gravityMultiplier = 3f;
        [Header("Impact Scaling")]
        [Tooltip("How much the fall speed increases slam impact (per unit of fall speed)")]
        public float impactSpeedFactor = 0.05f;

        [Tooltip("Maximum multiplier applied to damage/knockback from fall speed")]
        public float maxImpactScale = 3f;

        private FieldInfo verticalVelocityField;
        private float originalPlayerGravity;
        private bool gravityModified = false;

        private PlayerController player;
        private void Awake()
        {
            player = GetComponentInParent<PlayerController>();
            if (player == null)
            {
                player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
                player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            }

            // cache field info for manipulating player's vertical velocity when slamming midair
            verticalVelocityField = typeof(PlayerController).GetField("_verticalVelocity", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public override void Activate()
        {
            if (onCooldown) return;

            StartCoroutine(SlamRoutine());
        }

        private IEnumerator SlamRoutine()
        {
            onCooldown = true;

            // If player is midair, push them downward immediately so they slam faster
            if (player != null && !player.Grounded)
                // Ensure the player is pushed downward when slam activates so the slam reliably hits
                if (player != null)
                {
                    if (verticalVelocityField == null)
                    {
                        verticalVelocityField = typeof(PlayerController).GetField("_verticalVelocity", BindingFlags.NonPublic | BindingFlags.Instance);
                    }

                    if (verticalVelocityField != null)
                    {
                        verticalVelocityField.SetValue(player, airDropVelocity);
                        Debug.Log($"SlamDunkPerk: Applied air drop velocity {airDropVelocity} to player.");
                        float appliedVelocity = airDropVelocity * airDropStrength;
                        verticalVelocityField.SetValue(player, appliedVelocity);
                        Debug.Log($"SlamDunkPerk: Applied air drop velocity {appliedVelocity} to player (base {airDropVelocity} x {airDropStrength}).");
                    }

                    // If the player is currently grounded, nudge them downward a small amount so the engine registers a fall
                    if (player.Grounded)
                    {
                        var cc = player.GetComponent<CharacterController>();
                        if (cc != null)
                        {
                            // small immediate displacement downwards to ensure contact loss
                            // use a stronger immediate displacement scaled by airDropStrength
                            Vector3 push = Vector3.up * airDropVelocity * airDropStrength * 0.08f; // airDropVelocity is negative
                            cc.Move(push);
                        }
                    }

                    // temporarily increase gravity so the player falls faster
                    originalPlayerGravity = player.Gravity;
                    player.Gravity = originalPlayerGravity * gravityMultiplier;
                    gravityModified = true;
                }

            // push the player down immediately and wait for them to land
            Debug.Log("SlamDunkPerk: Forcing immediate drop and waiting to land...");

            float maxFallWait = 2.0f; // safety timeout in seconds
            float waited = 0f;
            while (player != null && !player.Grounded && waited < maxFallWait)
            {
                waited += Time.deltaTime;
                yield return null;
            }

            // restore gravity once landed or timeout
            if (player != null && gravityModified)
            {
                player.Gravity = originalPlayerGravity;
                gravityModified = false;
            }

         
            // determine impact scaling from player's vertical velocity (fall speed)
            float fallSpeed = 0f;
            if (player != null)
                fallSpeed = Mathf.Abs(player._verticalVelocity);

            float impactScale = 1f + fallSpeed * impactSpeedFactor;
            impactScale = Mathf.Clamp(impactScale, 1f, maxImpactScale);

            Vector3 center = transform.position;
            if (player != null)
            {
                center = player.transform.position;
            }

            // find colliders in radius
            Collider[] hits = Physics.OverlapSphere(center, radius);

            int hitCount = 0;
            foreach (var c in hits)
            {
                if (c == null) continue;

                // try to find an EnemyController on the collider or its parent
                EnemyController enemy = c.GetComponentInParent<EnemyController>();
                if (enemy == null) continue;

                // deal damage
                enemy.TakeDamage(damage);
                // deal damage scaled by impact
                int appliedDamage = Mathf.Max(1, Mathf.RoundToInt(damage * impactScale));
                enemy.TakeDamage(appliedDamage);
                hitCount++;

                // attempt to apply knockback if enemy has a rigidbody
                Rigidbody rb = c.attachedRigidbody ?? c.GetComponent<Rigidbody>();
                if (rb != null && knockbackForce > 0f)
                {
                    Vector3 away = (c.transform.position - center).normalized;
                    if (away.sqrMagnitude < 0.01f) away = Vector3.up;
                    rb.AddForce(away * knockbackForce, ForceMode.VelocityChange);
                    float appliedKnock = knockbackForce * impactScale;
                    rb.AddForce(away * appliedKnock, ForceMode.VelocityChange);
                }
                else
                {
                    // if enemy uses NavMeshAgent, try to nudge its destination away
                    var agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
                    if (agent != null && agent.isOnNavMesh)
                    {
                        Vector3 away = (c.transform.position - center).normalized;
                        if (away.sqrMagnitude < 0.01f) away = Vector3.back;
                        agent.SetDestination(c.transform.position + away * 2f);
                        agent.SetDestination(c.transform.position + away * (2f * impactScale));
                    }
                }
            }

            Debug.Log($"SlamDunkPerk: Slam hit {hitCount} enemies in radius {radius} for {damage} damage.");

            // notify listeners about slam landing
            try
            {
                OnSlamLanded?.Invoke(center, hits);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            // cooldown
            yield return new WaitForSeconds(cooldown);
            onCooldown = false;
        }

        private void OnDrawGizmosSelected()
        {
            // draw slam radius when selected in editor
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.35f);
            Vector3 center = transform.position;
            if (player != null) center = player.transform.position;
            Gizmos.DrawSphere(center, radius);
        }
    }
}
