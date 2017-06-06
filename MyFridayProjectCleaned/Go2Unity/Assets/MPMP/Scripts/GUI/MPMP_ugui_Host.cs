/*
* MPMP
* Copyright © 2016 Stefan Schlupek
* All rights reserved
* info@monoflow.org
*/

using UnityEngine;
using System.Collections;


namespace monoflow
{
    public class MPMP_ugui_Host : MonoBehaviour
    {

        public MPMP mpmpPlayer;
        void Awake()
        {
            if (mpmpPlayer == null) return;
            MPMP_ugui_Element[] elements = GetComponentsInChildren<MPMP_ugui_Element>();
            if (elements != null)
            {
                foreach (var e in elements)
                {
                    e.player = mpmpPlayer;
                }
            }
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
