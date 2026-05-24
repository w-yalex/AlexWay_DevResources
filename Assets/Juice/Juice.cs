using UnityEngine;
using System;


namespace AW.UnityResources
{
    /// <summary>
    /// Main utility script used to Play and Stop any JuiceComponent added on a GameObject.
    /// Optional overloads allow custom runtime data to be passed into specific JuiceComponents.
    /// If no custom data provided, the component will use its configured inspector settings.
    /// </summary>
    /// <example>
    /// Example usage for a MaterialFlash component:
    /// <code>
    /// Juice.PlayFeedbackOnObject<MaterialFlash>(gameObject, "RedFlash", () => Debug.Log($"Red Flash MaterialFlash on {gameObject} complete!"));
    /// </code>
    /// </example>
    public static class Juice
    {

        public static void PlayFeedbackOnObject<TComponent>(GameObject target, string referenceKey = null, Action onComplete = null)
            where TComponent : JuiceComponent
        {
            if (!IsComponentSearchValid<TComponent>(target, out var validComponents, referenceKey))
            {
                UnityDebug.LogWarning(typeof(Juice), $"Cannot call PlayFeedbackOnObject() for {typeof(TComponent)}");
                return;
            }

            foreach (var juiceComponent in validComponents)
                juiceComponent.PlayOnObject(onComplete);
        }
    
        
        public static void PlayFeedbackOnObject<TComponent, TData>(GameObject target, TData juiceData, string referenceKey = null, Action onComplete = null)
            where TComponent : JuiceComponent
            where TData : struct
        {

            if (!IsComponentSearchValid<TComponent>(target, out var validComponents, referenceKey))
            {
                UnityDebug.LogWarning(typeof(Juice), $"Cannot call PlayFeedbackOnObject() for {typeof(TComponent)}");
                return;
            }

            foreach (var juiceComponent in validComponents)
                juiceComponent.PlayOnObject(juiceData, onComplete);

        }


        public static void ClearFeedbackOnObject<TComponent>(GameObject target, string referenceKey = null)
            where TComponent : JuiceComponent
        {

            if (!IsComponentSearchValid<TComponent>(target, out var validComponents, referenceKey))
            {
                UnityDebug.LogWarning(typeof(Juice), $"Cannot call StopFeedbackOnObject() for {typeof(TComponent)}");
                return;
            }

            foreach (var juiceComponent in validComponents)
                juiceComponent.ClearOnObject();
        }


        private static bool IsComponentSearchValid<TComponent>(GameObject target, out TComponent[] validComponents, string referenceKey = null)
            where TComponent : JuiceComponent
        {
            validComponents = Array.Empty<TComponent>();
            if (!TryGetJuiceComponents<TComponent>(target, out var foundComponents))
            {
                UnityDebug.LogWarning(typeof(Juice), $"Invalid search, no {typeof(TComponent)} components found on {target.name}");
                return false;
            }

            if (referenceKey == null)
            {
                validComponents = foundComponents;
                return true;
            }
            else if (TryGetJuiceComponent<TComponent>(target, out var foundComponent, referenceKey))
            {
                validComponents = new [] { foundComponent };
                return true;
            }
            
            UnityDebug.LogWarning(typeof(Juice), $"Invalid search, no {typeof(TComponent).Name} component found on {target.name} with Reference Key: {referenceKey}");
            return false;
        }


        public static void PlayAllFeedbackOnObject(GameObject target)
        {
            var juiceComponents = target.GetComponents<JuiceComponent>();
            if (juiceComponents.Length == 0)
            {
                UnityDebug.LogWarning(typeof(Juice), $"Cannot call PlayAllFeedbackOnObject(), no JuiceComponents found on {target.name}");
                return;
            }

            foreach(var juiceComponent in juiceComponents)
                juiceComponent.PlayOnObject();
        }


        public static void ClearAllFeedbackOnObject(GameObject target)
        {
            var juiceComponents = target.GetComponents<JuiceComponent>();
            if (juiceComponents.Length == 0)
            {
                UnityDebug.LogWarning(typeof(Juice), $"Cannot call ClearAllFeedbackOnObject(), no JuiceComponents found on {target.name}");
                return;
            }

            foreach(var juiceComponent in juiceComponents)
                juiceComponent.ClearOnObject();
        }


        public static bool TryGetJuiceComponent<TComponent>(GameObject target, out TComponent foundComponent, string referenceKey = null)
            where TComponent : JuiceComponent
        {
            var matchingComponents = target.GetComponents<TComponent>();
            foundComponent = null;

            if (matchingComponents.Length == 0) return false;
            else if (matchingComponents.Length == 1)
            {
                bool isFound = referenceKey == null || referenceKey == matchingComponents[0].OptionalReferenceKey;
                if (isFound) foundComponent = matchingComponents[0];

                return isFound;
            }

            if (referenceKey == null)
            {
                UnityDebug.LogError(typeof(Juice), 
                    $"Multiple {typeof(TComponent)} JuiceComponents found on {target.name}, either specify reference key or use TryGetJuiceComponents()");
            }

            foreach(var juiceComponent in matchingComponents)
            {
                if (referenceKey == juiceComponent.OptionalReferenceKey)
                {
                    foundComponent = juiceComponent;
                    return true;
                }
            }

            return false;
        }


        public static bool TryGetJuiceComponents<TComponent>(GameObject target, out TComponent[] foundComponents)
            where TComponent : JuiceComponent
        {
            foundComponents = target.GetComponents<TComponent>();
            return foundComponents.Length > 0;
        }


    }

}
