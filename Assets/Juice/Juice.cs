using UnityEngine;
using System;


namespace AW.UnityResources
{
    public static class Juice
    {

        // Plays the component with the default settings configured on GameObject
        public static void PlayFeedbackOnObject<TComponent>(GameObject target, Action onComplete = null)
            where TComponent : JuiceComponent
        {
            if (!target.TryGetComponent(out TComponent juiceComponent))
            {
                UnityDebug.LogWarning(typeof(Juice), $"Cannot call PlayOnObject(), no {typeof(TComponent).Name} component found on {target.name}");
                return;
            }

            juiceComponent.PlayOnObject(onComplete);
        }


        // Pass in custom data at runtime based on the needs of each JuiceComponent
        public static void PlayFeedbackOnObject<TComponent, TData>(GameObject target, TData juiceData, Action onComplete = null)
            where TComponent : JuiceComponent
            where TData : struct
        {
            if (!target.TryGetComponent(out TComponent juiceComponent))
            {
                UnityDebug.LogWarning(typeof(Juice), $"Cannot call PlayOnObject(), no {typeof(TComponent).Name} component found on {target.name}");
                return;
            }

            juiceComponent.PlayOnObject(juiceData, onComplete);
        }


        public static void StopFeedbackOnObject<TComponent>(GameObject target)
            where TComponent : JuiceComponent
        {
            if (!target.TryGetComponent(out TComponent juiceComponent))
            {
                UnityDebug.LogWarning(typeof(Juice), $"Cannot call StopOnObject(), no {typeof(TComponent).Name} component found on {target.name}");
                return;
            }

            juiceComponent.StopOnObject();
        }


        public static void StopFeedbackOnObject<TComponent, TData>(GameObject target, TData juiceData)
            where TComponent : JuiceComponent
            where TData : struct
        {
            if (!target.TryGetComponent(out TComponent juiceComponent))
            {
                UnityDebug.LogWarning(typeof(Juice), $"Cannot call StopOnObject(), no {typeof(TComponent).Name} component found on {target.name}");
                return;
            }

            juiceComponent.StopOnObject(juiceData);
        }


        // Can only stop based on default
        public static void PlayAllFeedbackOnObject(GameObject target)
        {
            var juiceComponents = target.GetComponents<JuiceComponent>();
            if (juiceComponents.Length == 0)
            {
                UnityDebug.LogWarning(typeof(Juice), $"Cannot call PlayAll(), no JuiceComponents found on {target.name}");
                return;
            }

            foreach(var juiceComponent in juiceComponents)
                juiceComponent.PlayOnObject();
        }


        public static void StopAllFeedbackOnObject(GameObject target)
        {
            var juiceComponents = target.GetComponents<JuiceComponent>();
            if (juiceComponents.Length == 0)
            {
                UnityDebug.LogWarning(typeof(Juice), $"Cannot call StopAll(), no JuiceComponents found on {target.name}");
                return;
            }

            foreach(var juiceComponent in juiceComponents)
                juiceComponent.StopOnObject();
        }


        public static bool TryGetJuiceComponent<TComponent>(GameObject target, out JuiceComponent juiceComponent)
            where TComponent : JuiceComponent
            => target.TryGetComponent(out juiceComponent);


        public static bool TryGetJuiceComponents(GameObject target, out JuiceComponent[] juiceComponents)
        {
            juiceComponents = target.GetComponents<JuiceComponent>();
            return juiceComponents.Length > 0;
        }

 

    }

}
