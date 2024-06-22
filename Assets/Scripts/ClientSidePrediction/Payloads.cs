using Unity.Netcode;
using UnityEngine;

namespace Dropt
{
    public struct InputPayload : INetworkSerializable
    {
        public int tick;
        public Vector3 moveDirection;
        public Vector3 actionDirection;
        public PlayerAbilityEnum abilityTriggered;
        public PlayerAbilityEnum holdAbilityPending;
        public Hand abilityHand;
        public bool isHoldStartFlag;
        public bool isHoldFinishFlag;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref tick);
            serializer.SerializeValue(ref moveDirection);
            serializer.SerializeValue(ref actionDirection);
            serializer.SerializeValue(ref abilityTriggered);
            serializer.SerializeValue(ref holdAbilityPending);
            serializer.SerializeValue(ref abilityHand);
            serializer.SerializeValue(ref isHoldStartFlag);
            serializer.SerializeValue(ref isHoldFinishFlag);
        }
    }

    public struct StatePayload : INetworkSerializable
    {
        public int tick;
        public Vector3 position;
        public Vector3 velocity;
        public PlayerAbilityEnum abilityTriggered;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref tick);
            serializer.SerializeValue(ref position);
            serializer.SerializeValue(ref abilityTriggered);
        }
    }




}