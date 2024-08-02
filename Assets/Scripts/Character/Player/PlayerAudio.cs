using Audio.Game;
using Unity.Netcode;
using UnityEngine;

public sealed class PlayerAudio : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        GameAudioManager.Instance.PLAY_SOUND += OnPlaySound;

        if (IsServer && !IsHost)
            return;

        GameAudioManager.Instance.InitPlayer();
    }

    private void OnDestroy()
    {
        if (null == GameAudioManager.Instance)
            return;

        GameAudioManager.Instance.PLAY_SOUND -= OnPlaySound;
        GameAudioManager.Instance.DestroyPlayer();
    }

    private void OnPlaySound(string type, Vector3 position, ulong id)
    {
        if (!IsServer)
            return;

        PlaySoundClientRpc(type, position, id);
    }

    [Rpc(SendTo.NotMe)]
    void PlaySoundClientRpc(string type, Vector3 position, ulong id)
    {
        if (NetworkObjectId == id)
            return;

        GameAudioManager.Instance.PlaySoundForMe(type, position);
    }
}