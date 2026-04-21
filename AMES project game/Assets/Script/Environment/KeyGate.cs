using UnityEngine;

namespace AmesGame
{
    [RequireComponent(typeof(Collider))]
    public class KeyGate : MonoBehaviour
    {
        [Tooltip("If true the gate will only disappear when an object with a KeyHolder and HasKey=true touches it.")]
        public bool requireKey = true;

        [Tooltip("Optional sound to play when gate opens")]
        public AudioClip openSound;

        [Tooltip("Time in seconds to fade out before deactivating (0 = immediate)")]
        public float fadeDuration = 0.5f;

        private AudioSource audioSource;
        private Renderer[] renderers;
        private Collider col;

        private void Awake()
        {
            col = GetComponent<Collider>();
            col.isTrigger = true; // gate uses trigger
            renderers = GetComponentsInChildren<Renderer>(true);
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!requireKey)
            {
                OpenGate();
                return;
            }

            var kh = other.GetComponentInParent<KeyHolder>();
            if (kh != null && kh.HasKey)
            {
                OpenGate();
            }
        }

        public void OpenGate()
        {
            if (openSound != null)
            {
                audioSource.PlayOneShot(openSound);
            }

            // If the gate was opened by a player carrying a key, remove the key from the player and update UI
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                var kh = playerObj.GetComponent<KeyHolder>();
                if (kh != null && kh.HasKey)
                {
                    kh.HasKey = false;
                    var ui = playerObj.GetComponentInChildren<PlayerUI>();
                    if (ui == null) ui = FindObjectOfType<PlayerUI>();
                    if (ui != null) ui.SetHasKey(false);
                }
            }

            if (fadeDuration <= 0f)
            {
                gameObject.SetActive(false);
            }
            else
            {
                StartCoroutine(FadeAndDisable());
            }
        }

        private System.Collections.IEnumerator FadeAndDisable()
        {
            float elapsed = 0f;

            // collect initial materials and colors
            var mats = new Material[renderers.Length][];
            for (int i = 0; i < renderers.Length; i++)
            {
                var r = renderers[i];
                mats[i] = r.materials;
            }

            while (elapsed < fadeDuration)
            {
                float t = 1f - (elapsed / fadeDuration);
                for (int i = 0; i < renderers.Length; i++)
                {
                    var r = renderers[i];
                    foreach (var m in r.materials)
                    {
                        if (m.HasProperty("_Color"))
                        {
                            Color c = m.color;
                            c.a = t;
                            m.color = c;
                        }
                    }
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            gameObject.SetActive(false);
        }
    }
}
