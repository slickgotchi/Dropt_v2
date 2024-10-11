using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PoisonStack : MonoBehaviour
{
    private NetworkCharacter networkCharacter;
    private List<float> damagePerSecondList;
    private List<float> durationList;
    private int maxStackSize;
    private float elapsedTime;

    void Init()
    {
        networkCharacter = GetComponent<NetworkCharacter>();
        damagePerSecondList = new List<float>();
        durationList = new List<float>();
    }

    public static void ApplyPoisonStack(GameObject target, float damagePerSecond, float duration, int maxStackSize)
    {
        Debug.Log("Apply PoisonStack");
        PoisonStack poisonStack = target.GetComponent<PoisonStack>();
        if (poisonStack == null)
        {
            poisonStack = target.gameObject.AddComponent<PoisonStack>();
            poisonStack.Init();
        }
        poisonStack.AddStack(damagePerSecond, duration, maxStackSize);
    }

    private void AddStack(float damagePerSecond, float duration, int maxStackSize)
    {
        this.maxStackSize = maxStackSize;

        if (damagePerSecondList.Count < maxStackSize)
        {
            damagePerSecondList.Add(damagePerSecond);
            durationList.Add(duration);
        }
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;

        if (elapsedTime >= 0.5f)
        {
            ApplyDamage();
            elapsedTime = 0;
        }

        UpdateStacks();
    }

    private void ApplyDamage()
    {
        float totalDamage = 0f;
        for (int i = 0; i < damagePerSecondList.Count; i++)
        {
            totalDamage += damagePerSecondList[i] * 0.5f;
        }
        networkCharacter.TakeDamage(totalDamage, false);
    }

    private void UpdateStacks()
    {
        for (int i = 0; i < durationList.Count; i++)
        {
            durationList[i] -= Time.deltaTime;
        }

        for (int i = durationList.Count - 1; i >= 0; i--)
        {
            if (durationList[i] <= 0)
            {
                durationList.RemoveAt(i);
                damagePerSecondList.RemoveAt(i);
            }
        }

        // If there are no more stacks, remove the PoisonStack component
        if (durationList.Count == 0)
        {
            Destroy(this);
        }
    }
}