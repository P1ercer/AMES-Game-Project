using System.Collections;
using System.Reflection;
using UnityEngine;

namespace AmesGame
{
    // SlamDunkPerk: performs a slam centred on the player that deals AoE damage
    // on activation. Has a small windup, optional knockback, and a cooldown.
    public class SlamDunkPerk : Perk
    {
        [Tooltip("Damage dealt to each enemy in the AoE")]
        public int damage = 3;

        [Tooltip("Radius of the AoE (meters)")]
        public float radius = 5f;

        [Tooltip("Short windup before slam (seconds)")]
        public float windup = 0.25f;

        [Tooltip("Cooldown after using the slam (seconds)")]
        public float cooldown = 8f;

        [Tooltip("Optional knockback force applied to rigidbody enemies")]
        public float knockbackForce = 6f;

        private bool onCooldown = false;

        [Tooltip("Vertical velocity to apply to player when slamming while midair (negative = down)")]
        public float airDropVelocity = -25f;

        private FieldInfo verticalVelocityField;

        private PlayerController player;

        private void Awake()
        {
            player = GetComponentInParent<PlayerController>();
            if (player == null)
            {
                player = Object.FindAnyObjectByType<PlayerController>();
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
            {
                if (verticalVelocityField == null)
                {
                    verticalVelocityField = typeof(PlayerController).GetField("_verticalVelocity", BindingFlags.NonPublic | BindingFlags.Instance);
                }

                if (verticalVelocityField != null)
                {
                    verticalVelocityField.SetValue(player, airDropVelocity);
                    Debug.Log($"SlamDunkPerk: Applied air drop velocity {airDropVelocity} to player.");
                }
            }

            // brief windup so players can see/hear the slam
            Debug.Log($"SlamDunkPerk: Windup {windup}s before slam.");
            yield return new WaitForSeconds(windup);

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
                hitCount++;

                // attempt to apply knockback if enemy has a rigidbody
                Rigidbody rb = c.attachedRigidbody ?? c.GetComponent<Rigidbody>();
                if (rb != null && knockbackForce > 0f)
                {
                    Vector3 away = (c.transform.position - center).normalized;
                    if (away.sqrMagnitude < 0.01f) away = Vector3.up;
                    rb.AddForce(away * knockbackForce, ForceMode.VelocityChange);
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
                    }
                }
            }

            Debug.Log($"SlamDunkPerk: Slam hit {hitCount} enemies in radius {radius} for {damage} damage.");

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
