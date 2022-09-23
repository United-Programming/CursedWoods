using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonEyeCollider : MonoBehaviour {

    public Dragon dragon;
    
    private void OnTriggerEnter(Collider other) {
        int layer = 1 << other.gameObject.layer;
        if ((dragon.GetArrowLayerMask().value & layer) != 0) {
            dragon.EyeShoot();
        }
    }
}
