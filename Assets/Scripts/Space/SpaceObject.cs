using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Space
{
    public class SpaceObject : MonoBehaviour
    {

        public Vector3 Size { get; set; }
        public float Weight { get; set; }

        public SpaceObject()
        {
            Size = new Vector3(1.0f, 1.0f, 1.0f);
            Weight = 1.0f;
        }

    }
}
