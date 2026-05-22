using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

namespace AW.UnityResources
{

    public class TestMonobehaviour : MonoBehaviour
    {
        

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                var onPlayData = new TimeStop.OnPlay()
                {
                    PauseDuration = 2f
                };

                Juice.PlayAllFeedbackOnObject(gameObject);
            }
        }
    }

}
