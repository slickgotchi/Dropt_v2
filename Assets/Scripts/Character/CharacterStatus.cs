using Unity.Netcode;
using UnityEngine;

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

    public void SetPoisoned(bool value) => SetStatusEffect(PoisonedBit, value);
    public void SetStunned(bool value) => SetStatusEffect(StunnedBit, value);
    public void SetBurning(bool value) => SetStatusEffect(BurningBit, value);
    public void SetFrozen(bool value) => SetStatusEffect(FrozenBit, value);
    public void SetShielded(bool value) => SetStatusEffect(ShieldedBit, value);
    public void SetSilenced(bool value) => SetStatusEffect(SilencedBit, value);
    public void SetBleeding(bool value) => SetStatusEffect(BleedingBit, value);
    public void SetInvisible(bool value) => SetStatusEffect(InvisibleBit, value);
    public void SetCursed(bool value) => SetStatusEffect(CursedBit, value);
    public void SetRooted(bool value) => SetStatusEffect(RootedBit, value);
    public void SetConfused(bool value) => SetStatusEffect(ConfusedBit, value);
    public void SetEmpowered(bool value) => SetStatusEffect(EmpoweredBit, value);
    public void SetWeakened(bool value) => SetStatusEffect(WeakenedBit, value);
    public void SetHasted(bool value) => SetStatusEffect(HastedBit, value);
    public void SetSlowed(bool value) => SetStatusEffect(SlowedBit, value);
    public void SetAlerting(bool value) => SetStatusEffect(AlertingBit, value);

    private void SetStatusEffect(ushort bit, bool value)
    {
        if (IsServer)
        {
            if (value)
            {
                statusEffects.Value |= bit;
            }
            else
            {
                statusEffects.Value &= (ushort)~bit;
            }
        }
        else
        {
            SubmitStatusEffectServerRpc(bit, value);
        }
    }

    private bool HasStatusEffect(ushort bit)
    {
        return (statusEffects.Value & bit) != 0;
    }

    [ServerRpc]
    private void SubmitStatusEffectServerRpc(ushort bit, bool value)
    {
        if (value)
        {
            statusEffects.Value |= bit;
        }
        else
        {
            statusEffects.Value &= (ushort)~bit;
        }
    }
}
