using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Audio;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Dropt.Utils
{
    public static class AudioSourceExtensions
    {
        public static async UniTask FadeIn(this AudioSource source, float duration, bool showLogs)
        {
            if (showLogs)
            {
                Debug.Log("Chanel " + source.gameObject.name + " Fade In: " + source.clip);
            }

            source.DOKill();
            await source.DOFade(1, duration).ToUniTask();
        }

        public static async UniTask FadeOut(this AudioSource source, float duration, bool showLogs)
        {
            if (showLogs)
            {
                Debug.Log("Chanel " + source.gameObject.name + "Fade Out: " + source.clip);
            }
            
            bool isCompleted = false;

            var tween = source.DOFade(0, duration);
            
            tween.onComplete += () =>
            {
                isCompleted = true;
            };

            await tween.ToUniTask();
            
            if (!isCompleted)
            {
                return;
            }
            
            source.StopSingle();
        }

        public static bool TryGetFreeChannel(this IEnumerable<AudioSource> sources, out AudioSource channel)
        {
            channel = sources.FirstOrDefault(temp => temp.clip == null);
            return channel != default;
        }

        public static int CountFreeChannels(this IEnumerable<AudioSource> sources)
        {
            return sources.Count(temp => temp.clip == null);
        }

        public static bool TryGetBusyChannels(this IEnumerable<AudioSource> sources,
            out IEnumerable<AudioSource> channels)
        {
            channels = sources.Where(temp => temp.clip != null);
            return channels.Any();
        }

        public static int CountPlayingDuplicates(this IEnumerable<AudioSource> sources, string name)
        {
            int result = 0;

            foreach (var audioSource in sources)
            {
                if (audioSource.clip == null)
                    continue;

                if (audioSource.isPlaying && string.Equals(audioSource.clip.name, name))
                {
                    result++;
                }
            }

            return result;
        }

        public static void ParseSettings(this AudioSource source, ChannelEnvironmentSettings settings)
        {
            source.spatialBlend = settings.SpatialBlend;
            source.loop = settings.IsLoop;
            source.rolloffMode = settings.RolloffMode;
            source.maxDistance = settings.MaxDistance;
            source.minDistance = settings.MinDistance;
            source.dopplerLevel = settings.DopplerLevel;
        }

        public static void StopAll(this IEnumerable<AudioSource> sources)
        {
            if (sources == null || !sources.Any())
            {
                return;
            }

            foreach (var audioSource in sources)
            {
                audioSource.StopSingle();
            }
        }

        public static void StopSingle(this AudioSource source)
        {
            if (source == null)
            {
                return;
            }

            source.Stop();
            source.clip = null;
        }

        public static async UniTask ToUniTask(this AudioSource source, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            
            while (source != null && source.isPlaying)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                
                await UniTask.Yield();
            }
        }
    }
}