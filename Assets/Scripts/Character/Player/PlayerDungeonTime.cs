using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Netcode;

public class PlayerDungeonTime : NetworkBehaviour
{
    private NetworkVariable<int> m_hours = new NetworkVariable<int>(0);
    private NetworkVariable<int> m_minutes = new NetworkVariable<int>(0);
    private NetworkVariable<int> m_seconds = new NetworkVariable<int>(0);

    private bool m_allowToStartTimer;

    private CancellationTokenSource m_cancellationTokenSource;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer) return;
        m_seconds.OnValueChanged += OnSeondsChanged;
        m_minutes.OnValueChanged += OnMinuteChanged;
    }

    private void OnMinuteChanged(int previousValue, int newValue)
    {
        if (newValue >= 60)
        {
            m_minutes.Value = 0;
            m_hours.Value++;
        }
    }

    private void OnSeondsChanged(int previousValue, int newValue)
    {
        if (newValue >= 60)
        {
            m_seconds.Value = 0;
            m_minutes.Value++;
        }
    }

    public async void StartTimer()
    {
        if (m_allowToStartTimer) return;

        m_cancellationTokenSource = new CancellationTokenSource();
        m_allowToStartTimer = true;

        while (m_allowToStartTimer)
        {
            try
            {
                m_seconds.Value++;
                await UniTask.Delay(1000, cancellationToken: m_cancellationTokenSource.Token);
            }
            catch
            {
                break;
            }
        }
    }

    public void StopTimer()
    {
        m_allowToStartTimer = false;
        m_cancellationTokenSource?.Cancel();
    }

    public void ResetTimer()
    {
        m_hours.Value = m_minutes.Value = m_hours.Value = 0;
    }

    public override string ToString()
    {
        return $"{m_hours.Value:00} hr : {m_minutes.Value:00} min : {m_seconds.Value:00} s";
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        StopTimer();
    }
}