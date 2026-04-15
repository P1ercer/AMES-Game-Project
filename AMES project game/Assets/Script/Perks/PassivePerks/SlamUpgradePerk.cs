using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AmesGame
{
    public class SlamUpgradePerk : Perk
    {
        [Header("Perk Info")]

        [Header("Slam Buffs")]
        public float damageMultiplier = 1.5f;
        public float radiusMultiplier = 1.25f;
        [Tooltip("Factor applied to cooldown (use <1 to reduce cooldown)")]
        public float cooldownMultiplier = 0.8f;
        [Tooltip("Additional multiplier to air drop velocity (more negative to drop faster)")]
        public float airDropMultiplier = 1.1f;

        private SlamDunkPerk slam;

        private int originalDamage;
        private float originalRadius;
        private float originalCooldown;
        private float originalKnockback;
        private float originalAirDrop;

        private bool applied = false;

        [Header("Extra DOT on slam landing")]
        [Tooltip("Damage per tick applied to nearby enemies after slam")]
        public int enemyDotPerTick = 2;

        [Tooltip("Damage per tick applied to player after slam")]
        public int playerDotPerTick = 1;

        [Tooltip("Seconds between DOT ticks")]
        public float dotInterval = 1f;

        [Tooltip("Total duration of DOT in seconds")]
        public float dotDuration = 5f;

        private PlayerController player;

        // track active DOT coroutines so we can cancel them on unequip
        private HashSet<EnemyController> enemiesWithDot = new HashSet<EnemyController>();
        private List<Coroutine> activeCoroutines = new List<Coroutine>();

        public override void OnEquip()
        {
            base.OnEquip();

            if (applied) return;

            // try to find SlamDunkPerk on this GameObject or its children
            slam = GetComponentInChildren<SlamDunkPerk>();

            // if not found, try to locate via PerkController slots on this object
            if (slam == null)
            {
                var pc = GetComponent<PerkController>();
                if (pc != null)
                {
                    foreach (var slot in pc.perkSlots)
                    {
                        if (slot != null && slot.perk is SlamDunkPerk)
                        {
                            slam = slot.perk as SlamDunkPerk;
                            break;
                        }
                    }
                }
            }

            if (slam == null)
            {
                Debug.Log("GroundpoundPerk: No SlamDunkPerk found to buff.");
                return;
            }

            // store originals
            originalDamage = slam.damage;
            originalRadius = slam.radius;
            originalCooldown = slam.cooldown;
            originalKnockback = slam.knockbackForce;
            originalAirDrop = slam.airDropVelocity;

            // apply buffs
            slam.damage = Mathf.Max(1, Mathf.RoundToInt(slam.damage * damageMultiplier));
            slam.radius *= radiusMultiplier;
            slam.cooldown *= cooldownMultiplier;
            slam.airDropVelocity *= airDropMultiplier;

            applied = true;

            Debug.Log($"SlamUpgradePerk: Buffed SlamDunk (dmg {originalDamage}->{slam.damage}, radius {originalRadius}->{slam.radius}).");

            // subscribe to slam landed event to apply DOT
            slam.OnSlamLanded += OnSlamLanded;
        }

        public override void OnUnequip()
        {
            base.OnUnequip();

            if (!applied || slam == null) return;

            // restore originals
            slam.damage = originalDamage;
            slam.radius = originalRadius;
            slam.cooldown = originalCooldown;
            slam.knockbackForce = originalKnockback;
            slam.airDropVelocity = originalAirDrop;
            // unsubscribe
            slam.OnSlamLanded -= OnSlamLanded;

            // cancel active DOT coroutines
            foreach (var co in activeCoroutines)
            {
                if (co != null) StopCoroutine(co);
            }
            activeCoroutines.Clear();
            enemiesWithDot.Clear();

            applied = false;

            Debug.Log("SlamUpgradePerk: Removed SlamDunk buffs.");
        }

        private void OnSlamLanded(Vector3 center, Collider[] hits)
        {
            // apply DOT to enemies in hits and DOT to player
            // enemies get stronger DOT
            foreach (var c in hits)
            {
                if (c == null) continue;
                var enemy = c.GetComponentInParent<EnemyController>();
                if (enemy == null) continue;

                if (!enemiesWithDot.Contains(enemy))
                {
                    enemiesWithDot.Add(enemy);
                    var co = StartCoroutine(ApplyDotToEnemy(enemy, enemyDotPerTick, dotInterval, dotDuration));
                    activeCoroutines.Add(co);
                }
            }

            // apply DOT to player as a drawback
            player = GetComponent<PlayerController>();
            if (player != null)
            {
                var pco = StartCoroutine(ApplyDotToPlayer(player, playerDotPerTick, dotInterval, dotDuration));
                activeCoroutines.Add(pco);
            }
        }

        private IEnumerator ApplyDotToEnemy(EnemyController enemy, int dmgPerTick, float interval, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (enemy == null) break;
                enemy.TakeDamage(dmgPerTick);
                yield return new WaitForSeconds(interval);
                elapsed += interval;
            }

            enemiesWithDot.Remove(enemy);
        }

        private IEnumerator ApplyDotToPlayer(PlayerController p, int dmgPerTick, float interval, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (p == null) break;
                p.TakeDamage(dmgPerTick);
                yield return new WaitForSeconds(interval);
                elapsed += interval;
            }
        }
    }
}