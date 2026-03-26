using System.Collections.Generic;
using UnityEngine;

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
        public string perkName = "New Perk"; //default fallback name

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
        }

        public List<PerkSlot> perkSlots = new List<PerkSlot>();

        [SerializeField] private int maxActivePerks = 2;
        [SerializeField] private int maxPassivePerks = 1;

        public int MaxActivePerks => maxActivePerks;
        public int MaxPassivePerks => maxPassivePerks;

        private void Start()
        {
            foreach (var slot in perkSlots)
            {
                if (slot == null || slot.perk == null)
                {
                    Debug.LogWarning("PerkController: Empty perk slot detected in inspector.");
                    continue;
                }

                if (string.IsNullOrEmpty(slot.perk.perkName))
                {
                    slot.perk.perkName = slot.perk.name;
                    Debug.LogWarning($"Perk '{slot.perk.name}' had no perkName, using GameObject name instead.");
                }

                if (slot.chosen)
                {
                    Debug.Log($"Starting with perk: {slot.perk.perkName}");
                    slot.perk.OnEquip();
                }
            }
        }

        private void Update()
        {
            foreach (var slot in perkSlots)
            {
                if (slot == null || slot.perk == null || !slot.chosen)
                    continue;

                if (slot.mode == PerkMode.Passive)
                {
                    slot.perk.Tick();
                }
                else
                {
                    if (IsKeyPressed(slot.activationKey))
                    {
                        Debug.Log($"Activating perk: {slot.perk.perkName}");
                        slot.perk.Activate();
                    }
                }
            }
        }

        bool IsKeyPressed(ActivationKey key)
        {
            switch (key)
            {
                case ActivationKey.Shift:
                    return Input.GetKeyDown(KeyCode.LeftShift);
                case ActivationKey.Ctrl:
                    return Input.GetKeyDown(KeyCode.LeftControl);
                case ActivationKey.Q:
                    return Input.GetKeyDown(KeyCode.Q);
                default:
                    return false;
            }
        }

        public void AddPerk(Perk perk)
        {
            if (perk == null)
                return;

            PerkSlot found = null;

            foreach (var s in perkSlots)
            {
                if (s != null && s.perk == perk)
                {
                    found = s;
                    break;
                }
            }

            if (found == null)
            {
                Debug.LogWarning($"PerkController: Tried to add a perk not in slots: {perk.name}");
                return;
            }

            if (found.chosen)
                return;

            int activeCount = 0;
            int passiveCount = 0;

            foreach (var s in perkSlots)
            {
                if (s != null && s.chosen)
                {
                    if (s.mode == PerkMode.Active) activeCount++;
                    else passiveCount++;
                }
            }

            if (found.mode == PerkMode.Active && activeCount >= maxActivePerks)
            {
                Debug.LogWarning($"Cannot add active perk '{perk.perkName}': max ({maxActivePerks}) reached.");
                return;
            }

            if (found.mode == PerkMode.Passive && passiveCount >= maxPassivePerks)
            {
                Debug.LogWarning($"Cannot add passive perk '{perk.perkName}': max ({maxPassivePerks}) reached.");
                return;
            }

            found.chosen = true;

            Debug.Log($"Added perk: {perk.perkName}");

            found.perk.OnEquip();
        }

        public void RemovePerk(Perk perk)
        {
            if (perk == null) return;

            foreach (var s in perkSlots)
            {
                if (s != null && s.perk == perk)
                {
                    if (s.chosen)
                    {
                        Debug.Log($"Removed perk: {perk.perkName}");
                        s.perk.OnUnequip();
                    }

                    s.chosen = false;
                    break;
                }
            }
        }
    }
}