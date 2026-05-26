using UnityEngine;
using System;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

namespace AW.UnityResources
{
    /// <summary>
    /// Optional script that organizes all the JuiceComponents on the GameObject 
    /// without drag and drop from assets folder
    /// </summary>
    public class ObjectFeedback : MonoBehaviour
    {
        public enum Feedback
        {
            None,
            TimeScaleFreeze,
            TimeScaleSlowMotion,
            TimeScaleAccelerate,
            CinemachineCameraShake,
            ScreenFlashStrobe,
            ChromaticAberrationPulse,
            LensDistortionBounce,
            MaterialFlash,
            SquashAndStretch,
            PhysicsKnockback,
            SoundEffect,
            ParticleEffect,
        }
    
        [Header("Feedback Components")]
        [SerializeField] private List<Feedback>_feedbackOnObject = new(){ Feedback.None };

        private void OnValidate()
        {
            EditorApplication.delayCall -= UpdateObjectFeedbackHierarchy;
            EditorApplication.delayCall += UpdateObjectFeedbackHierarchy;
        }
        

        private void OnDestroy()
        {
            EditorApplication.delayCall -= UpdateObjectFeedbackHierarchy;
        }

        [ContextMenu("Refresh ObjectFeedback Hierarchy")]
        private void RefreshObjectFeedbackHierarchy()
            => UpdateObjectFeedbackHierarchy();
        

        private void UpdateObjectFeedbackHierarchy()
        {
            if (!this) return;

            MoveObjectFeedbackComponentToBottom();
            UpdateActiveJuiceComponentsOnGameObject();
            OrderJuiceComponentsBelowObjectFeedback();
            
        }

        private void UpdateActiveJuiceComponentsOnGameObject()
        {

            foreach(Feedback feedback in Enum.GetValues(typeof(Feedback)))
            {
                if (feedback == Feedback.None) continue;

                // bool shouldExist = (_feedbackOnObject & feedback) != 0;

                Type feedbackType = Type.GetType($"AW.UnityResources.{feedback}");
                if (feedbackType == null || !feedbackType.IsSubclassOf(typeof(JuiceComponent)))
                {
                    UnityDebug.LogWarning(this, $"Incorrect enum naming, no valid juice component of {feedback} found");
                    continue;
                }

                Component existingComponent = GetComponent(feedbackType);
                bool doesExist = existingComponent != null;

                // if (shouldExist && !doesExist)
                // {
                //     gameObject.AddComponent(feedbackType);
                // }
                // else if (!shouldExist && doesExist)
                // {
                //     DestroyImmediate(existingComponent);
                // }

            }
            
        }

        private void MoveObjectFeedbackComponentToBottom()
        {
            Component objectFeedback = GetComponent<ObjectFeedback>();
            while (ComponentUtility.MoveComponentDown(objectFeedback)){ }
        }


        private void OrderJuiceComponentsBelowObjectFeedback()
        {

            foreach (Feedback feedback in Enum.GetValues(typeof(Feedback)))
            {
                if (feedback == Feedback.None) continue;

                Type feedbackType = Type.GetType($"AW.UnityResources.{feedback}");
                if (feedbackType == null) continue; // Will alreayd throw warning when when adding and removing componets

                Component component = GetComponent(feedbackType);
                if (component == null) continue;

                while (ComponentUtility.MoveComponentDown(component)){ }
            }
        }


    }

}
