using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGroup : MonoBehaviour
{
    public int minNumberLevels;
    public int maxNumberLevels;
    public List<Level.NetworkLevel> networkLevels = new List<Level.NetworkLevel>();
}
