using System;
using Chest;
using Unity.Netcode;
using UnityEngine;
using Random = System.Random;

namespace Interactables
{
    public class Chest : Interactable
    {
        private ChestConfig m_config;
        
        private Random m_random;
        private int MinCGHST => m_config.MinCGHST;
        private int MaxCGHST => m_config.MaxCGHST;
        private int MinGltr => m_config.MinGltr;
        private int MaxGltr => m_config.MaxGltr;
        private PickupItemManager OrbsFactory => PickupItemManager.Instance;
        private float OrbsSpawnRange => m_config.OrbsSpawnRange;
        private Random Random => m_random ??= new Random();

        public void SetUp(ChestConfig config)
        {
            m_config = config;
        }

        public override void OnStartInteraction()
        {
            base.OnStartInteraction();

            SetActiveTextBox(true);
        }

        public override void OnUpdateInteraction()
        {
            base.OnUpdateInteraction();

            if (!Input.GetKeyDown(KeyCode.F)) return;

            SetActiveTextBox(false);

            SpawnOrbsRpc();

            DespawnChestServerRpc();
        }

        public override void OnFinishInteraction()
        {
            base.OnFinishInteraction();

            SetActiveTextBox(false);
        }

        private Vector3 GetOrbsRandomPosition(Vector3 center, float radius)
        {
            var angle = (float)GetRandomNumber(0f, Math.PI * 2);

            float x = center.x + Mathf.Cos(angle) * radius;
            float y = center.y + Mathf.Sin(angle) * radius;

            return new Vector3(x, y, 1);
        }

        private double GetRandomNumber(double minimum, double maximum)
        {
            return Random.NextDouble() * (maximum - minimum) + minimum;
        }

        private void SetActiveTextBox(bool value)
        {
            InteractableUICanvas.Instance.InteractTextbox.SetActive(value);
        }

        [Rpc(SendTo.Server)]
        private void SpawnOrbsRpc()
        {
            var actualGltr = Random.Next(MinGltr, MaxGltr);
            var actualCGHST = Random.Next(MinCGHST, MaxCGHST);

            var center = transform.position;

            var gltrPosition = GetOrbsRandomPosition(center, OrbsSpawnRange);
            var CGHSTPosition = GetOrbsRandomPosition(center, OrbsSpawnRange);

            OrbsFactory.SpawnGltr(actualGltr, gltrPosition);

            for (int i = 0; i < actualCGHST; i++)
            {
                OrbsFactory.SpawnSmallCGHST(CGHSTPosition);
            }
        }

        [Rpc(SendTo.Server)]
        private void DespawnChestServerRpc()
        {
            NetworkObject.Despawn();
        }
    }
}