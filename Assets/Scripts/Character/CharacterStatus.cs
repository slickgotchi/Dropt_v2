using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class CharacterStatus : NetworkBehaviour
{
    private const ushort PoisonedBit = 1 << 0;   // 0000 0000 0000 0001
    private const ushort StunnedBit = 1 << 1;    // 0000 0000 0000 0010
    private const ushort BurningBit = 1 << 2;    // 0000 0000 0000 0100
    private const ushort FrozenBit = 1 << 3;     // 0000 0000 0000 1000
    private const ushort ShieldedBit = 1 << 4;   // 0000 0000 0001 0000
    private const ushort SilencedBit = 1 << 5;   // 0000 0000 0010 0000
    private const ushort BleedingBit = 1 << 6;   // 0000 0000 0100 0000
    private const ushort InvisibleBit = 1 << 7;  // 0000 0000 1000 0000
    private const ushort CursedBit = 1 << 8;     // 0000 0001 0000 0000
    private const ushort RootedBit = 1 << 9;     // 0000 0010 0000 0000
    private const ushort ConfusedBit = 1 << 10;  // 0000 0100 0000 0000
    private const ushort EmpoweredBit = 1 << 11; // 0000 1000 0000 0000
    private const ushort WeakenedBit = 1 << 12;  // 0001 0000 0000 0000
    private const ushort HastedBit = 1 << 13;    // 0010 0000 0000 0000
    private const ushort SlowedBit = 1 << 14;    // 0100 0000 0000 0000
    private const ushort AlertingBit = 1 << 15;  // 1000 0000 0000 0000

    private NetworkVariable<ushort> statusEffects = new NetworkVariable<ushort>();
    private Dictionary<ushort, float> statusEffectEndTimes = new Dictionary<ushort, float>();

    private void Update()
    {
        if (IsServer)
        {
            List<ushort> keys = new List<ushort>(statusEffectEndTimes.Keys);
            foreach (ushort bit in keys)
            {
                if (Time.time >= statusEffectEndTimes[bit])
                {
                    SetStatusEffect(bit, false);
                    statusEffectEndTimes.Remove(bit);
                }
            }
        }
    }

    public bool IsPoisoned() => HasStatusEffect(PoisonedBit);
    public bool IsStunned() => HasStatusEffect(StunnedBit);
    public bool IsBurning() => HasStatusEffect(BurningBit);
    public bool IsFrozen() => HasStatusEffect(FrozenBit);
    public bool IsShielded() => HasStatusEffect(ShieldedBit);
    public bool IsSilenced() => HasStatusEffect(SilencedBit);
    public bool IsBleeding() => HasStatusEffect(BleedingBit);
    public bool IsInvisible() => HasStatusEffect(InvisibleBit);
    public bool IsCursed() => HasStatusEffect(CursedBit);
    public bool IsRooted() => HasStatusEffect(RootedBit);
    public bool IsConfused() => HasStatusEffect(ConfusedBit);
    public bool IsEmpowered() => HasStatusEffect(EmpoweredBit);
    public bool IsWeakened() => HasStatusEffect(WeakenedBit);
    public bool IsHasted() => HasStatusEffect(HastedBit);
    public bool IsSlowed() => HasStatusEffect(SlowedBit);
    public bool IsAlerting() => HasStatusEffect(AlertingBit);

    public void SetPoisoned(bool value, float duration = 0) => SetStatusEffect(PoisonedBit, value, duration);
    public void SetStunned(bool value, float duration = 0) => SetStatusEffect(StunnedBit, value, duration);
    public void SetBurning(bool value, float duration = 0) => SetStatusEffect(BurningBit, value, duration);
    public void SetFrozen(bool value, float duration = 0) => SetStatusEffect(FrozenBit, value, duration);
    public void SetShielded(bool value, float duration = 0) => SetStatusEffect(ShieldedBit, value, duration);
    public void SetSilenced(bool value, float duration = 0) => SetStatusEffect(SilencedBit, value, duration);
    public void SetBleeding(bool value, float duration = 0) => SetStatusEffect(BleedingBit, value, duration);
    public void SetInvisible(bool value, float duration = 0) => SetStatusEffect(InvisibleBit, value, duration);
    public void SetCursed(bool value, float duration = 0) => SetStatusEffect(CursedBit, value, duration);
    public void SetRooted(bool value, float duration = 0) => SetStatusEffect(RootedBit, value, duration);
    public void SetConfused(bool value, float duration = 0) => SetStatusEffect(ConfusedBit, value, duration);
    public void SetEmpowered(bool value, float duration = 0) => SetStatusEffect(EmpoweredBit, value, duration);
    public void SetWeakened(bool value, float duration = 0) => SetStatusEffect(WeakenedBit, value, duration);
    public void SetHasted(bool value, float duration = 0) => SetStatusEffect(HastedBit, value, duration);
    public void SetSlowed(bool value, float duration = 0) => SetStatusEffect(SlowedBit, value, duration);
    public void SetAlerting(bool value, float duration = 0) => SetStatusEffect(AlertingBit, value, duration);

    private void SetStatusEffect(ushort bit, bool value, float duration = 0)
    {
        if (IsServer)
        {
            if (value)
            {
                statusEffects.Value |= bit;
                if (duration > 0)
                {
                    statusEffectEndTimes[bit] = Time.time + duration;
                }
            }
            else
            {
                statusEffects.Value &= (ushort)~bit;
                statusEffectEndTimes.Remove(bit);
            }
        }
        else
        {
            SubmitStatusEffectServerRpc(bit, value, duration);
        }
    }

    private bool HasStatusEffect(ushort bit)
    {
        return (statusEffects.Value & bit) != 0;
    }

    [ServerRpc]
    private void SubmitStatusEffectServerRpc(ushort bit, bool value, float duration)
    {
        SetStatusEffect(bit, value, duration);
    }
}
