using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonAnimationEvents : MonoBehaviour {
    private Dragon dragon;

    public void MovingWingsDown() {
        if (dragon != null) dragon.MovingWingsDown();
    }

    public void ReachGlideWingPosition() {
        if (dragon != null) dragon.ReachGlideWingPosition();
    }

    public void SetDragon(Dragon d) => dragon = d;
}
