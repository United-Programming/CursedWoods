using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieAnimationEvents : MonoBehaviour
{
    public Zombie zombie;

    public void AttackCompleted() {
        if (zombie!=null) zombie.AttackCompleted();
    }
    
    public void AttackStarted() {
        if (zombie != null) zombie.AttackStarted();
    }
    
    public void Step() {
        if (zombie != null) zombie.Step();
    }
}
