using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> m_levels = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        if (m_levels.Count <= 0)
        {
            Debug.Log("Add a level to the LevelManager!");
        }

        // spawn first level
        Instantiate(m_levels[0]);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
