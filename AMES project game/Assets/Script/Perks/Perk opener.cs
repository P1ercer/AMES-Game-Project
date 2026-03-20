using UnityEngine;

namespace AmesGame
{
    [RequireComponent(typeof(EnemyController))]
    public class PerkOpener : MonoBehaviour
    {
        private EnemyController enemy;
        private PerkChooserUI perkUI;

        [Header("Settings")]
        public bool triggerOnDeath = true;

        private void Awake()
        {
            enemy = GetComponent<EnemyController>();

            if (perkUI == null)
                perkUI = FindFirstObjectByType<PerkChooserUI>();
        }

        private void OnEnable()
        {
            if (enemy != null)
                enemy.OnEnemyDied += HandleEnemyDeath;
        }

        private void OnDisable()
        {
            if (enemy != null)
                enemy.OnEnemyDied -= HandleEnemyDeath;
        }

        public void HandleEnemyDeath()
        {
            if (!triggerOnDeath) return;

            if (perkUI != null)
            {
                perkUI.ShowPerkUI();
            }
            else
            {
                Debug.LogWarning("PerkOpener: No PerkChooserUI found in scene.");
            }
        }
    }
}