using UnityEngine;
using System;

namespace AW.UnityResources
{
    [Serializable]
    public class AudioSourceData
    {
        public AudioClip AudioClip;

        [Range(0f, 1f)] public float MinVolume = 1f;
        [Range(0f, 1f)] public float MaxVolume = 1f;

        [Range(-3f, 3f)] public float MinPitch = 1f;
        [Range(-3f, 3f)] public float MaxPitch = 1f;

        public bool IsLooping;
        public bool IsSpatial;
        public Vector3 SpawnPos;
        public Transform ParentTarget;

        [Tooltip("Primarily used for looping audio but can also be used to cut short one sound")]
        public float CustomPlayDuration = -1f;

    }

}