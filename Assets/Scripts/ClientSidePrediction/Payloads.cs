using Unity.Netcode;
using UnityEngine;

namespace Dropt
{
    public struct InputPayload : INetworkSerializable
    {
        public int tick;
        public Vector3 moveDirection;
        public Vector3 actionDirection;
        //public bool isDash;
        public float teleportDistance;
        public float slowFactor;
        public int slowFactorExpiryTick;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref tick);
            serializer.SerializeValue(ref moveDirection);
            serializer.SerializeValue(ref actionDirection);
            //serializer.SerializeValue(ref isDash);
            serializer.SerializeValue(ref teleportDistance);
            serializer.SerializeValue(ref slowFactor);
            serializer.SerializeValue(ref slowFactorExpiryTick);
        }
    }

    public struct StatePayload : INetworkSerializable
    {
        public int tick;
        public Vector3 position;
        public Vector3 velocity;
        //public bool isDash;
        public float teleportDistance;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref tick);
            serializer.SerializeValue(ref position);
            serializer.SerializeValue(ref velocity);
            //serializer.SerializeValue(ref isDash);
            serializer.SerializeValue(ref teleportDistance);
        }
    }

    public struct NonNetworkStatePayload
    {
        public int tick;
        public ulong networkObjectId;
        public Vector3 position;
        public Vector3 velocity;
    }
}