using UnityEngine;

namespace AmesGame
{
    // Gatekeeper: an enemy that behaves like EnemyController but gives the player a key when it dies.
    public class Gatekeeper : EnemyController
    {
        [Tooltip("Optional sound to play when giving the key")]
        public AudioClip giveKeySound;

        private AudioSource audioSource;

        private void Awake()
        {
            // subscribe to the instance death event defined in EnemyController
            this.OnEnemyDied += HandleDeathGiveKey;

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        private void HandleDeathGiveKey()
        {
            // Find the player
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj == null) return;

            // Give the key to the player's KeyHolder (attach one if missing)
            var kh = playerObj.GetComponent<KeyHolder>();
            if (kh == null)
            {
                kh = playerObj.AddComponent<KeyHolder>();
            }

            kh.HasKey = true;

            // Update PlayerUI to show the key (if present)
            var ui = playerObj.GetComponentInChildren<PlayerUI>();
            if (ui == null)
            {
                // try scene-wide
                ui = FindObjectOfType<PlayerUI>();
            }

            if (ui != null)
            {
                ui.SetHasKey(true);
            }

            if (giveKeySound != null && audioSource != null)
            {
                audioSource.PlayOneShot(giveKeySound);
            }
        }

        private void OnDestroy()
        {
            // clean up subscription to avoid leaks in editor
            this.OnEnemyDied -= HandleDeathGiveKey;
        }
    }
}
