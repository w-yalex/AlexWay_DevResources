using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Cinemachine;

namespace AW.UnityResources
{

    public class TestScript1 : MonoBehaviour
    {

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
          
                Juice.PlayAllFeedbackOnObject(gameObject);

            }

        }

    }

}
