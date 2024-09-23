using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GotchiHub
{
    public class ThirdwebCanvas : DroptCanvas
    {
        public static ThirdwebCanvas Instance {  get; private set; }

        private void Awake()
        {
            Instance = this;
        }
    }
}
