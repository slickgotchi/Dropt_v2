using Unity.Netcode;
using UnityEngine;

namespace Dropt
{
    public struct InputPayload : INetworkSerializable
    {
        public int tick;
        public ulong networkObjectId;
        public Vector3 moveDirection;
        public Vector3 actionDirection;
        public bool isDash;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref tick);
            serializer.SerializeValue(ref networkObjectId);
            serializer.SerializeValue(ref moveDirection);
            serializer.SerializeValue(ref actionDirection);
            serializer.SerializeValue(ref isDash);
        }
    }

    public struct StatePayload : INetworkSerializable
    {
        public int tick;
        public ulong networkObjectId;
        public Vector3 position;
        public Vector3 velocity;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref tick);
            serializer.SerializeValue(ref networkObjectId);
            serializer.SerializeValue(ref position);
            serializer.SerializeValue(ref velocity);
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