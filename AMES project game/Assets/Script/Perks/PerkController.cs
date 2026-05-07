using UnityEngine;
using System.Collections.Generic;

namespace AmesGame
{
    public enum ActivationKey
    {
        Shift,
        Ctrl,
        Q
    }

    public enum PerkMode
    {
        Active,
        Passive
    }

    public abstract class Perk : MonoBehaviour
    {
        [Header("Perk Info")]
        public string perkName = "New Perk";

        [TextArea(2, 5)]
        public string description;

        public Sprite icon;

        [Header("Cooldown")]
        [Tooltip("Cooldown time in seconds after Activate is used (0 = no cooldown)")]
        public float cooldown = 0f;

        public virtual void Activate() { }

        public virtual void OnEquip()
        {
            Debug.Log($"Equipped perk: {perkName}");
        }

        public virtual void OnUnequip()
        {
            Debug.Log($"Unequipped perk: {perkName}");
        }

        public virtual void Tick() { }
    }

    public class PerkController : MonoBehaviour
    {
        [System.Serializable]
        public class PerkSlot
        {
            public Perk perk;
            public PerkMode mode = PerkMode.Active;
            public ActivationKey activationKey = ActivationKey.Shift;
            public bool chosen = false;

            [Header("Selection Settings")]
            [Range(0f, 100f)]
            public float weight = 1f;

            public bool includeInRandom = true;

            [Header("Audio")]
            [Tooltip("Optional audio clip played when this slot's active perk is triggered")]
            public AudioClip activateClip;
            [Range(0f, 1f)]
            public float clipVolume = 1f;

            // runtime cooldown remaining in seconds (0 = ready)
            [HideInInspector]
            public float cooldownRemaining = 0f;
        }

        public List<PerkSlot> perkSlots = new List<PerkSlot>();

        [SerializeField] private int maxActivePerks = 2;
        [SerializeField] private int maxPassivePerks = 1;

        public int MaxActivePerks => maxActivePerks;
        public int MaxPassivePerks => maxPassivePerks;

        [Header("Audio (fallback)")]
        [Tooltip("Fallback sound played when an active perk is triggered and the slot has no clip")]
        public AudioClip activePerkActivateSound;
        private AudioSource perkAudioSource;

        private void Awake()
        {
            // ensure we have an AudioSource to play perk-related feedback
            perkAudioSource = GetComponent<AudioSource>();
            if (perkAudioSource == null)
            {
                perkAudioSource = gameObject.AddComponent<AudioSource>();
                perkAudioSource.playOnAwake = false;
            }
        }

        private void Start()
        {
            foreach (var slot in perkSlots)
            {
                if (slot == null || slot.perk == null)
                {
                    Debug.LogWarning("PerkController: Empty perk slot detected.");
                    continue;
                }

                if (string.IsNullOrEmpty(slot.perk.perkName))
                {
                    slot.perk.perkName = slot.perk.name;
                }

                if (slot.chosen)
                {
                    slot.perk.OnEquip();
                }

                // ensure cooldownRemaining initialized
                slot.cooldownRemaining = 0f;
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            foreach (var slot in perkSlots)
            {
                if (slot == null || slot.perk == null || !slot.chosen)
                    continue;

                // decrement cooldown timer (works for active and passive, but only used for active)
                if (slot.cooldownRemaining > 0f)
                {
                    slot.cooldownRemaining = Mathf.Max(0f, slot.cooldownRemaining - dt);
                }

                if (slot.mode == PerkMode.Passive)
                {
                    slot.perk.Tick();
                }
                else if (IsKeyPressed(slot.activationKey))
                {
                    // If slot has cooldown remaining, ignore activation
                    if (slot.cooldownRemaining > 0f)
                        continue;

                    // Play slot-specific activation clip if provided, otherwise use fallback
                    if (slot.activateClip != null)
                    {
                        if (perkAudioSource != null)
                            perkAudioSource.PlayOneShot(slot.activateClip, slot.clipVolume);
                        else
                            AudioSource.PlayClipAtPoint(slot.activateClip, transform.position, slot.clipVolume);
                    }
                    else if (activePerkActivateSound != null)
                    {
                        if (perkAudioSource != null)
                            perkAudioSource.PlayOneShot(activePerkActivateSound);
                        else
                            AudioSource.PlayClipAtPoint(activePerkActivateSound, transform.position);
                    }

                    // invoke the active perk
                    slot.perk.Activate();

                    // set cooldown based on perk's configured cooldown
                    slot.cooldownRemaining = Mathf.Max(0f, slot.perk.cooldown);
                }
            }
        }

        bool IsKeyPressed(ActivationKey key)
        {
            switch (key)
            {
                case ActivationKey.Shift: return Input.GetKeyDown(KeyCode.LeftShift);
                case ActivationKey.Ctrl: return Input.GetKeyDown(KeyCode.LeftControl);
                case ActivationKey.Q: return Input.GetKeyDown(KeyCode.Q);
                default: return false;
            }
        }

        //Skewed chances
        public Perk GetRandomPerk()
        {
            float totalWeight = 0f;

            foreach (var slot in perkSlots)
            {
                if (IsValid(slot))
                    totalWeight += slot.weight;
            }

            if (totalWeight <= 0f)
                return null;

            float randomPoint = Random.Range(0f, totalWeight);

            foreach (var slot in perkSlots)
            {
                if (!IsValid(slot)) continue;

                if (randomPoint < slot.weight)
                    return slot.perk;

                randomPoint -= slot.weight;
            }

            return null;
        }

        bool IsValid(PerkSlot slot)
        {
            return slot != null &&
                   slot.perk != null &&
                   !slot.chosen &&
                   slot.includeInRandom &&
                   slot.weight > 0f;
        }

        public void AddRandomPerk()
        {
            var perk = GetRandomPerk();
            if (perk != null)
                AddPerk(perk);
        }

        public void AddPerk(Perk perk)
        {
            if (perk == null) return;

            PerkSlot found = null;

            foreach (var s in perkSlots)
            {
                if (s != null && s.perk == perk)
                {
                    found = s;
                    break;
                }
            }

            if (found == null || found.chosen)
                return;

            int active = 0;
            int passive = 0;

            foreach (var s in perkSlots)
            {
                if (s != null && s.chosen)
                {
                    if (s.mode == PerkMode.Active) active++;
                    else passive++;
                }
            }

            if (found.mode == PerkMode.Active && active >= maxActivePerks) return;
            if (found.mode == PerkMode.Passive && passive >= maxPassivePerks) return;

            found.chosen = true;
            found.perk.OnEquip();
        }

        public void RemovePerk(Perk perk)
        {
            foreach (var s in perkSlots)
            {
                if (s != null && s.perk == perk)
                {
                    if (s.chosen)
                        s.perk.OnUnequip();

                    s.chosen = false;
                    break;
                }
            }
        }

        // Expose a safe accessor for UI to read cooldown remaining for a slot index.
        public float GetCooldownRemainingForSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= perkSlots.Count) return 0f;
            var s = perkSlots[slotIndex];
            if (s == null || s.perk == null) return 0f;
            return s.cooldownRemaining;
        }
    }
}