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

            // If a reference wasn't assigned in inspector, try to auto-detect a PerkChooserUI in the scene.
            if (perkUI == null)
            {
                // First try the simple runtime lookup (returns active objects)
                perkUI = FindObjectOfType<PerkChooserUI>();

                // If not found (or may be inactive), fall back to Resources lookup and pick the first one in a loaded scene.
                if (perkUI == null)
                {
                    var all = Resources.FindObjectsOfTypeAll<PerkChooserUI>();
                    foreach (var p in all)
                    {
                        // ensure the object belongs to a scene (not a prefab asset) and that the scene is loaded
                        if (p == null) continue;
                        if (p.gameObject.scene.IsValid() && p.gameObject.scene.isLoaded)
                        {
                            perkUI = p;
                            break;
                        }
                    }
                }
            }
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