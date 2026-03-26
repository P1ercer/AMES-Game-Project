using System.Collections;
using UnityEngine;

namespace AmesGame
{
    public class InARushPerk : Perk
    {
        [Header("Rush Settings")]
        public float speedPerKill = 0.5f;        // Speed added per kill
        public float boostDuration = 5f;         // Duration of the speed boost
        public float slowDuration = 2f;          // Duration of the slowdown after boost
        public float slowMultiplier = 0.7f;      // Speed multiplier when slowed

        private PlayerController player;

        private void Awake()
        {
            player = GetComponent<PlayerController>();
        }

        public override void OnEquip()
        {
            base.OnEquip();
            EnemyController.OnEnemyKilled += HandleEnemyKilled;
        }

        public override void OnUnequip()
        {
            base.OnUnequip();
            EnemyController.OnEnemyKilled -= HandleEnemyKilled;
        }

        private void HandleEnemyKilled(GameObject enemy)
        {
            if (enemy.CompareTag("Enemy"))
            {
                if (player != null)
                {
                    StartCoroutine(BoostAndSlowRoutine());
                }
            }
        }

        private IEnumerator BoostAndSlowRoutine()
        {
            // Apply speed boost
            player.MoveSpeed += speedPerKill;

            // Wait for boost duration
            yield return new WaitForSeconds(boostDuration);

            // Apply temporary slow
            float originalSpeed = player.MoveSpeed;
            player.MoveSpeed *= slowMultiplier;

            yield return new WaitForSeconds(slowDuration);

            // Restore speed
            player.MoveSpeed = originalSpeed;
        }
    }
}