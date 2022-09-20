using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : MonoBehaviour {
    public float speed = 20f;
    
    private Vector3 endPosition;
    
    public enum Status {
        Idle,
        StartMoving,
    }

    public Status status = Status.Idle;

    // Update is called once per frame
    void Update()
    {
        if (status == Status.Idle) return;

        if (HasArrivedToEndPos()) {
            Destroy(gameObject);
        }

        transform.position = Vector3.MoveTowards(transform.position, endPosition, speed * Time.deltaTime);
    }

    public void SetTarget(Vector3 targetPosition) {
        endPosition = targetPosition;
        status = Status.StartMoving;
    }
    
    private bool HasArrivedToEndPos() => Vector3.Distance(transform.position, endPosition) < 1;
}
