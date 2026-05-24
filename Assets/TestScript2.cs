using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

namespace AW.UnityResources
{

    public class TestScript2 : MonoBehaviour
    {

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Juice.PlayAllFeedbackOnObject(gameObject);
            }

        }
    }

}
