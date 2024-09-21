using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GotchiHub
{
    public class ThirdwebCanvas : MonoBehaviour
    {
        public static ThirdwebCanvas Instance {  get; private set; }

        public GameObject Container;

        private void Awake()
        {
            Instance = this;

            Container.SetActive(false);
        }
    }
}
