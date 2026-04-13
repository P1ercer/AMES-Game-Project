using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace AmesGame
{
    // GrapplePerk: pulls the nearest enemy within range towards the player over a short duration.
    public class GrapplePerk : Perk
    {
        [Tooltip("Maximum range to search for enemies")]
        public float range = 20f;

        [Tooltip("How long the pull effect lasts (seconds)")]
        public float pullDuration = 1.25f;

        [Tooltip("Speed used to pull the enemy towards the player")]
        public float pullSpeed = 10f;

        [Tooltip("Cooldown after using the grapple")]
        public float cooldown = 5f;

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

            // find nearest enemy within range
            EnemyController[] enemies = Object.FindObjectsOfType<EnemyController>();
            EnemyController nearest = null;
            float best = float.MaxValue;
            Vector3 origin = player != null ? player.transform.position : transform.position;

            foreach (var e in enemies)
            {
                if (e == null) continue;
                float d = Vector3.SqrMagnitude(e.transform.position - origin);
                if (d < best && d <= range * range)
                {
                    best = d;
                    nearest = e;
                }
            }

            if (nearest == null)
            {
                Debug.Log("GrapplePerk: No enemy in range to grapple.");
                return;
            }

            StartCoroutine(PullRoutine(nearest));
        }

        private IEnumerator PullRoutine(EnemyController enemy)
        {
            if (enemy == null) yield break;
            onCooldown = true;

            Debug.Log($"GrapplePerk: Pulling '{enemy.gameObject.name}' towards player for {pullDuration} seconds.");

            float elapsed = 0f;

            Rigidbody rb = enemy.GetComponent<Rigidbody>();
            NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();

            while (elapsed < pullDuration && enemy != null)
            {
                Vector3 target = player != null ? player.transform.position : transform.position;
                Vector3 dir = (target - enemy.transform.position).normalized;

                if (rb != null)
                {
                    // set velocity towards player
                    rb.linearVelocity = dir * pullSpeed;
                }
                else if (agent != null && agent.isOnNavMesh)
                {
                    // steer agent towards player
                    agent.SetDestination(target);
                }
                else
                {
                    // fallback: move transform directly
                    enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, target, pullSpeed * Time.deltaTime);
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            // stop rigidbody movement to avoid overshoot
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
            }

            Debug.Log($"GrapplePerk: Finished pulling '{enemy?.gameObject.name}'. Starting cooldown {cooldown}s.");

            // cooldown
            yield return new WaitForSeconds(cooldown);
            onCooldown = false;
        }
    }
}
