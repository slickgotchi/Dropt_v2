using System;
using System.Linq;
using Chest;
using Chests.WeightRandom;
using Unity.Netcode;
using UnityEngine;
using Random = System.Random;

namespace Interactables
{
    public class Chest : Interactable
    {
        private ChestConfig m_config;

        [SerializeField] private WeaponSwap m_prefab;
        [SerializeField] private int m_minCGHST;
        [SerializeField] private int m_maxCGHST;
        [SerializeField] private int m_minGltr;
        [SerializeField] private int m_maxGltr;

        private WearableManager WearableManager => WearableManager.Instance;
        private PickupItemManager OrbsFactory => PickupItemManager.Instance;
        private float OrbsSpawnRange => m_config.OrbsSpawnRange;
        private int ItemsSpawnCount => m_config.ItemsDropCount;
        private ActivePlayersData[] PlayersToPercents => m_config.PlayersToPercent;

        private WeightVariable<Wearable.RarityEnum>[] WeaponsRarity => m_config.Weapons;

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

            SpawnWearablesRpc();

            DespawnChestRpc();
        }

        public override void OnFinishInteraction()
        {
            base.OnFinishInteraction();

            SetActiveTextBox(false);
        }

        private Vector3 GetRandomPosition(Vector3 center, float radius)
        {
            var random = new Random();
            var angle = (float)GetRandomNumber(random, 0f, Math.PI * 2);

            float x = center.x + Mathf.Cos(angle) * radius;
            float y = center.y + Mathf.Sin(angle) * radius;

            return new Vector3(x, y, 1);
        }

        private double GetRandomNumber(Random random, double minimum, double maximum)
        {
            return random.NextDouble() * (maximum - minimum) + minimum;
        }

        private void SetActiveTextBox(bool value)
        {
            InteractableUICanvas.Instance.InteractTextbox.SetActive(value);
        }

        private int CalculateAdditionalSpectrals()
        {
            var playersCount = NetworkManager.ConnectedClients.Count;

            if (playersCount <= 1)
            {
                return 0;
            }

            var itemsCount = PlayersToPercents.ToWeightArray(playersCount).GetRandom();

            return itemsCount;
        }

        [Rpc(SendTo.Server)]
        private void SpawnOrbsRpc()
        {
            if (!IsServer)
            {
                return;
            }
            
            var random = new Random();
            var actualGltr = random.Next(m_minGltr, m_maxGltr);
            var actualCGHST = random.Next(m_minCGHST, m_maxCGHST);

            var center = transform.position;

            var gltrPosition = GetRandomPosition(center, OrbsSpawnRange);
            var CGHSTPosition = GetRandomPosition(center, OrbsSpawnRange);

            OrbsFactory.SpawnGltr(actualGltr, gltrPosition);

            for (int i = 0; i < actualCGHST; i++)
            {
                OrbsFactory.SpawnSmallCGHST(CGHSTPosition);
            }
        }

        [Rpc(SendTo.Server)]
        private void SpawnWearablesRpc()
        {
            if (!IsServer)
            {
                return;
            }

            var actualCountToSpawn = ItemsSpawnCount + CalculateAdditionalSpectrals();

            for (int i = 0; i < actualCountToSpawn; i++)
            {
                var rarity = WeaponsRarity.GetRandom();

                if (rarity == Wearable.RarityEnum.NA)
                {
                    return;
                }

                var rawItems = WearableManager.wearablesByNameEnum.Values.ToArray();

                var item = rawItems.FirstOrDefault(temp =>
                    temp.Rarity == rarity && temp.WeaponType is not Wearable.WeaponTypeEnum.NA);

                if (item == default)
                {
                    return;
                }

                var position = GetRandomPosition(transform.position, 1f);

                var swap = Instantiate(m_prefab, position, Quaternion.identity);
                
                swap.SyncNameEnum.Value = item.NameType;
                
                swap.NetworkObject.Spawn();
            }
        }
        
        [Rpc(SendTo.Server)]
        private void DespawnChestRpc()
        {
            if (!IsServer)
            {
                return;
            }

            NetworkObject.Despawn();
        }
    }
}