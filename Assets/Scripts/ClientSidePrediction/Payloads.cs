using Unity.Netcode;
using UnityEngine;

namespace Dropt
{
    public struct InputPayload : INetworkSerializable
    {
        public int tick;
        public Vector3 moveDirection;
        public Vector3 actionDirection;
        public float actionDistance;
        public PlayerAbilityEnum triggeredAbilityEnum;
        public PlayerAbilityEnum holdStartTriggeredAbilityEnum;
        public Hand abilityHand;
        public bool isHoldStartFlag;
        public bool isHoldFinishFlag;
        public bool isMovementEnabled;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref tick);
            serializer.SerializeValue(ref moveDirection);
            serializer.SerializeValue(ref actionDirection);
            serializer.SerializeValue(ref actionDistance);
            serializer.SerializeValue(ref triggeredAbilityEnum);
            serializer.SerializeValue(ref holdStartTriggeredAbilityEnum);
            serializer.SerializeValue(ref abilityHand);
            serializer.SerializeValue(ref isHoldStartFlag);
            serializer.SerializeValue(ref isHoldFinishFlag);
            serializer.SerializeValue(ref isMovementEnabled);
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