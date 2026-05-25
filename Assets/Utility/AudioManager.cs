using UnityEngine;
using DG.Tweening;


namespace AW.UnityResources
{
    
    public class AudioManager : MonoBehaviour
    {
        private const int DefaultCapacity = 40;
        private const int MaxSize = 60;
        
        public static PoolSource<AudioSource> AudioEmitterPoolSource { get; private set; } 
        private static readonly object _audioPlayTarget = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitAudioEmitterPool()
        {
            GameObject newAudioSourceEmitterObj = new GameObject("AudioSource_Emitter");
            AudioSource audioSource = newAudioSourceEmitterObj.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;

            AudioEmitterPoolSource.OverrideCollection(audioSource, DefaultCapacity, MaxSize);
        }


        public static PoolHandle<AudioSource>Play(AudioSourceData audioSourceData)
        {

            AudioSource audioSource = GetConfiguredAudioSource(audioSourceData);
            audioSource.Play();

            float releaseTimer = -1f;

            if (audioSourceData.CustomPlayDuration != -1)
                releaseTimer = audioSourceData.CustomPlayDuration;
            else if (!audioSourceData.IsLooping)
                releaseTimer = audioSourceData.AudioClip.length / Mathf.Abs(audioSource.pitch);


            if (releaseTimer != -1)
            {
                DOVirtual.DelayedCall(releaseTimer, () => AudioEmitterPoolSource.Collection.Release(audioSource))
                    .SetTarget(_audioPlayTarget);
            }

            return new PoolHandle<AudioSource>(audioSource, AudioEmitterPoolSource);

        }


        private static AudioSource GetConfiguredAudioSource(AudioSourceData audioSourceData)
        {
            AudioSource audioSource = AudioEmitterPoolSource.Collection.Get();
            
            audioSource.transform.position = audioSourceData.SpawnPos;

            audioSource.clip = audioSourceData.AudioClip;
            audioSource.spatialBlend = audioSourceData.IsSpatial ? 1f : 0f;
            
            audioSource.loop = audioSourceData.IsLooping;
            audioSource.pitch = Random.Range(audioSourceData.MinPitch, audioSourceData.MaxPitch);
            audioSource.volume = Random.Range(audioSourceData.MinVolume, audioSourceData.MaxVolume);

            return audioSource;
        }




    }

}
