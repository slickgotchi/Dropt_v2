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

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref tick);
            serializer.SerializeValue(ref moveDirection);
            serializer.SerializeValue(ref actionDirection);
            serializer.SerializeValue(ref abilityTriggered);
        }
    }

    public struct StatePayload : INetworkSerializable
    {
        public int tick;
        public Vector3 position;
        //public Vector3 velocity;
        public PlayerAbilityEnum abilityTriggered;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref tick);
            serializer.SerializeValue(ref position);
            //serializer.SerializeValue(ref velocity);
            serializer.SerializeValue(ref abilityTriggered);
        }
    }




}