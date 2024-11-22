using UnityEngine;

public class SoundFX_ProjectileHitGround : MonoBehaviour
{
    [SerializeField] private SoundData m_soundData;

    public void Play()
    {
        AudioManager.Instance.PlaySpatialSFX(m_soundData.AudioClip,
                                             transform.position,
                                             false,
                                             m_soundData.Volume);
    }
}
