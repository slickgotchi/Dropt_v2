using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class LevelGroup : MonoBehaviour
{
    public int minNumberLevels = 1;
    public int maxNumberLevels = 1;
    [SerializeField] private List<Level.NetworkLevel> m_levels = new List<Level.NetworkLevel>();

    [HideInInspector] public List<Level.NetworkLevel> levels = new List<Level.NetworkLevel>();

    public void Shuffle()
    {
        levels.Clear();

        // make copy of setLevels into levels
        levels = new List<Level.NetworkLevel>(m_levels);

        ShuffleList();
        SetRandomizedListLength();
    }

    private void ShuffleList()
    {
        for (int i = levels.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            Level.NetworkLevel temp = levels[i];
            levels[i] = levels[randomIndex];
            levels[randomIndex] = temp;
        }
    }

    private void SetRandomizedListLength()
    {
        maxNumberLevels = math.min(maxNumberLevels, levels.Count);
        minNumberLevels = math.min(minNumberLevels, maxNumberLevels);

        var numberLevels = UnityEngine.Random.Range(minNumberLevels, maxNumberLevels+1);
        for (int i = levels.Count - 1; i >= numberLevels; i--)
        {
            levels.RemoveAt(i);
        }
    }
}
