using UnityEngine;
using DG.Tweening;


namespace AW.UnityResources
{
    public static class TimeScale
    {
        public enum Modifier
        {
            None,
            Accelerate,
            SlowMotion,
            Freeze
        }

        private static Modifier _activeModifier = Modifier.None;
        private static int _currentPriority = -1;

        public static readonly object ModifierTarget = new();

        public static float GetCurrent() => Time.timeScale;

        public static void Override(float newValue) => Time.timeScale = newValue;

        public static bool TrySetActiveModifier(Modifier modifier, int overridePriority)
        {
            bool isOverriden = false;

            if (_currentPriority == -1 && (int)modifier > (int)_activeModifier)
                isOverriden = true;
            else if (overridePriority > _currentPriority)
                isOverriden = true;

            if (isOverriden)
            {
                _activeModifier = modifier;
                _currentPriority = overridePriority;
            }

           return isOverriden;
        }

        public static bool TryClearActiveModifier(Modifier modifier)
        {
            if (modifier != _activeModifier) return false;

            _activeModifier = Modifier.None;
            _currentPriority = -1;

            return true;
        }

        
    }
}