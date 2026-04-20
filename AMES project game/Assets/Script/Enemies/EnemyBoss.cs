using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace AmesGame
{
    // EnemyBoss: derived from EnemyController but can have player-like perks toggled on.
    // Perks implemented: Slam (area knockback/damage), HandCannon (fires a projectile, applies self-knockback), ForceField (temporary invulnerability)
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyBoss : EnemyController
    {
        [Header("Perk Toggles")]
        public bool hasSlam = false;
        public bool hasHandCannon = false;
        public bool hasForceField = false;

        [Header("Slam Settings")]
        public float slamRadius = 4f;
        public int slamDamage = 2;
        public float slamKnockback = 6f;
        public float slamCooldown = 6f;
        [Header("Slam Jump")]
        [Tooltip("Chance (0..1) that the boss will attempt a jump when performing a slam")]
        public float slamJumpChance = 0.35f;
        [Tooltip("Upward force applied to the boss when it attempts to jump (world units)")]
        public float slamJumpForce = 4f;

        [Header("HandCannon Settings")]
        public GameObject handCannonProjectile;
        public Transform handCannonSpawn;
        public float handCannonProjectileSpeed = 12f;
        public float handCannonCooldown = 2f;
        public float handCannonRecoil = 3f;

        [Header("ForceField Settings")]
        public float forceFieldDuration = 3f;
        public float forceFieldCooldown = 10f;
        [Tooltip("If true the boss will attempt to use perks from range (without needing to get close)")]
        public bool usePerksAtRange = true;
        [Tooltip("Range within which boss will attempt ranged perks such as handcannon or slam")]
        public float perkRange = 10f;
        [Tooltip("Chance (0..1) to trigger forcefield when performing another perk")]
        public float forceFieldOnPerkChance = 0.25f;

        private float slamNext = 0f;
        private float handNext = 0f;
        private float fieldNext = 0f;

        private bool forceActive = false;

        protected override void Update()
        {
            base.Update(); // reuse movement and shooting logic

            // simple AI: attempt perks either when in range (configured) or when very close
            var player = GameObject.FindGameObjectWithTag("Player");
            float dist = player != null ? Vector3.Distance(transform.position, player.transform.position) : float.MaxValue;

            if (hasSlam && Time.time >= slamNext && dist <= (usePerksAtRange ? perkRange : 6f))
            {
                StartCoroutine(PerformSlam());
                slamNext = Time.time + slamCooldown;
            }

            if (hasHandCannon && Time.time >= handNext && dist <= (usePerksAtRange ? perkRange : 12f))
            {
                if (player != null)
                {
                    FireHandCannonAt(player.transform.position);
                    handNext = Time.time + handCannonCooldown;
                }
            }

            if (hasForceField && Time.time >= fieldNext)
            {
                StartCoroutine(ActivateForceField());
                fieldNext = Time.time + forceFieldCooldown;
            }
        }

        private IEnumerator PerformSlam()
        {
            // small windup
            yield return new WaitForSeconds(0.25f);

            // randomly attempt to jump as part of the slam
            bool willJump = Random.value <= slamJumpChance;
            if (willJump)
            {
                // prefer Rigidbody upward impulse if present
                var rbSelf = GetComponent<Rigidbody>();
                if (rbSelf != null)
                {
                    rbSelf.AddForce(Vector3.up * slamJumpForce, ForceMode.VelocityChange);
                }
                else
                {
                    // try nav agent move or small transform nudge upwards
                    var agent = GetComponent<NavMeshAgent>();
                    if (agent != null)
                    {
                        agent.Move(Vector3.up * (slamJumpForce * 0.1f));
                    }
                    else
                    {
                        transform.position += Vector3.up * (slamJumpForce * 0.1f);
                    }
                }
            }

            Vector3 center = transform.position;
            Collider[] hits = Physics.OverlapSphere(center, slamRadius);

            foreach (var c in hits)
            {
                if (c == null) continue;
                var p = c.GetComponentInParent<PlayerController>();
                if (p != null)
                {
                    // chance to activate forcefield proactively when performing a perk
                    if (hasForceField && !forceActive && Random.value <= forceFieldOnPerkChance)
                    {
                        StartCoroutine(ActivateForceField());
                    }

                    // apply damage via player's TakeDamage
                    p.TakeDamage(slamDamage);

                    // compute impulse (horizontal + upward)
                    Vector3 away = (p.transform.position - center).normalized;
                    Vector3 horizontalImpulse = new Vector3(away.x, 0f, away.z) * (slamKnockback * 0.5f);
                    float verticalImpulse = slamKnockback * 0.75f; // upward velocity equivalent

                    // prefer Rigidbody-based physics knockback like HandCannonperk
                    var rbPlayer = p.GetComponent<Rigidbody>();
                    if (rbPlayer != null)
                    {
                        Vector3 impulse = horizontalImpulse + Vector3.up * verticalImpulse;
                        rbPlayer.AddForce(impulse, ForceMode.VelocityChange);
                    }
                    else
                    {
                        // fallback to PlayerController API which adjusts velocities
                        p.ApplyKnockback(horizontalImpulse, verticalImpulse);
                    }
                }

                var enemy = c.GetComponentInParent<EnemyController>();
                if (enemy != null && enemy != this)
                {
                    // apply small knockback to other enemies
                    var rb = c.attachedRigidbody ?? c.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        Vector3 away = (c.transform.position - center).normalized;
                        // include slight upwards component for other enemies
                        Vector3 k = away * (slamKnockback * 0.5f) + Vector3.up * (slamKnockback * 0.25f);
                        rb.AddForce(k, ForceMode.VelocityChange);
                    }
                }
            }
        }

        private void FireHandCannonAt(Vector3 target)
        {
            if (handCannonProjectile == null || handCannonSpawn == null) return;

            var proj = Instantiate(handCannonProjectile, handCannonSpawn.position, handCannonSpawn.rotation);
            var pd = proj.GetComponent<ProjectileDamage>() ?? proj.AddComponent<ProjectileDamage>();
            pd.damage = 1;

            var rb = proj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 dir = (target - handCannonSpawn.position).normalized;
                rb.linearVelocity = dir * handCannonProjectileSpeed;
            }

            Destroy(proj, 6f);

            // recoil on self via agent.Move or transform
            // recoil on self: prefer physics (Rigidbody), otherwise NavMeshAgent or transform fallback
            var rbSelf = GetComponent<Rigidbody>();
            var agent = GetComponent<NavMeshAgent>();
            Vector3 recoilImpulse = -transform.forward * handCannonRecoil;

            bool createdRb = false;
            if (rbSelf == null)
            {
                // create a temporary Rigidbody to allow physics-based recoil if none exists
                rbSelf = gameObject.AddComponent<Rigidbody>();
                rbSelf.mass = 1f;
                rbSelf.constraints = RigidbodyConstraints.FreezeRotation; // keep upright
                createdRb = true;
            }

            // apply physics recoil and ensure NavMeshAgent (if present) does not fight physics briefly
            if (agent != null && agent.enabled)
            {
                StartCoroutine(ApplyPhysicsRecoil(rbSelf, agent, recoilImpulse, createdRb));
            }
            else
            {
                rbSelf.AddForce(recoilImpulse, ForceMode.VelocityChange);

                // if we created a temporary Rigidbody, schedule its removal after a short time so normal movement resumes
                if (createdRb)
                {
                    StartCoroutine(RemoveTemporaryRigidbody(rbSelf, 0.3f));
                }
            }
        }

        private System.Collections.IEnumerator ApplyPhysicsRecoil(Rigidbody rbSelf, NavMeshAgent agent, Vector3 impulse, bool temporaryRb)
        {
            // disable agent so physics can move the body without being overridden
            bool wasEnabled = agent.enabled;
            agent.enabled = false;

            rbSelf.AddForce(impulse, ForceMode.VelocityChange);

            // let physics simulate for a short moment
            float physTime = 0.2f;
            float t = 0f;
            while (t < physTime)
            {
                t += Time.deltaTime;
                yield return null;
            }

            // snap agent to rb position and re-enable
            agent.Warp(rbSelf.position);
            agent.enabled = wasEnabled;

            if (temporaryRb)
            {
                // remove temporary rigidbody after short delay
                StartCoroutine(RemoveTemporaryRigidbody(rbSelf, 0.25f));
            }
        }

        private System.Collections.IEnumerator RemoveTemporaryRigidbody(Rigidbody rb, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (rb != null)
            {
                Destroy(rb);
            }
        }

        private IEnumerator ActivateForceField()
        {
            forceActive = true; // Activate force field
            yield return new WaitForSeconds(forceFieldDuration); // Wait for duration
            forceActive = false; // Deactivate force field
        }

        public override void TakeDamage(int damage)
        {
            if (forceActive) return; // negate damage while forcefield active

            // If health drops below half due to this hit, immediately activate forcefield (if available)
            float prev = health;
            base.TakeDamage(damage);
            float after = health;

            if (hasForceField && !forceActive)
            {
                // detect crossing half health
                float half = maxHealth * 0.5f;
                if (prev > half && after <= half)
                {
                    StartCoroutine(ActivateForceField());
                    // after forced activation, we allow it to continue using randomly after duration (existing logic)
                }
            }
        }
    }
}
