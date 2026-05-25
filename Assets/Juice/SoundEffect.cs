using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using System;


namespace AW.UnityResources
{
    public class SoundEffect : JuiceComponent
    {  
        [Header("Sound Effect")]
        [SerializeField] private AudioSourceData _defaultAudioSourceData;
        
        private PoolHandle<AudioSource> _activeAudioHandle;
        private Action<AudioSource> _onAudioRelease;

        private void Awake()
        {
            if (!_defaultAudioSourceData.AudioClip)
            {
                UnityDebug.LogWarning(this, $"No AudioClip configured in default audio source data for {gameObject.name} gameObject");

            }
        }

        private void OnDisable()
        {
            if (_activeAudioHandle != null)
            {
                _activeAudioHandle.PoolSource.OnComponentRelease -= _onAudioRelease;
            }
        }


        public override void PlayOnObject(Action onComplete = null)
            => PlaySoundEffect(_defaultAudioSourceData, onComplete);
    

        public override void PlayOnObject<TData>(TData juiceData, Action onComplete = null)
        {
            if (juiceData is not AudioSourceData customAudioSourceData)
            {
                UnityDebug.LogWarning(this, $"Juice data received did not match {typeof(AudioSourceData).FullName}");
                return;
            }

            PlaySoundEffect(customAudioSourceData, onComplete);
        }

        public override void ClearOnObject() => ClearSoundEffect();

        private void PlaySoundEffect(AudioSourceData audioSourceData, Action onComplete = null)
        {
            _activeAudioHandle = AudioManager.Play(audioSourceData);           

            _onAudioRelease = (audioSource) =>
            {
                if (audioSource != _activeAudioHandle.ActiveComponent) return;

                _activeAudioHandle.PoolSource.OnComponentRelease -= _onAudioRelease;
                _activeAudioHandle = null;
                _onAudioRelease = null;

                OnSoundEffectComplete(audioSourceData, onComplete);
            };

            
            _activeAudioHandle.PoolSource.OnComponentRelease += _onAudioRelease;

        }


        private void ClearSoundEffect()
        {
            if (_activeAudioHandle == null) return;

            _activeAudioHandle.PoolSource.OnComponentRelease -= _onAudioRelease;
            _activeAudioHandle.TryRelease();

            _activeAudioHandle = null;
            _onAudioRelease = null;
        }


        private void OnSoundEffectComplete(AudioSourceData audioSourceData, Action onComplete = null)
        {
            onComplete?.Invoke();
        }



    }

}
