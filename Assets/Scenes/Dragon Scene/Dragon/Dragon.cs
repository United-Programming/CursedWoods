using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor.PackageManager;
using UnityEngine;
using Random = UnityEngine.Random;

public class Dragon : MonoBehaviour {
    
    
    public DragonLevel level;
    // public Animator anim;
    public float speed = 5f;
    public float minFlyingAltitude; // make the wings won't clip with the terrain.
    public float maxFlyingAltitude; // make the dragon be inside the camera.
    public float defaultFlyingAltitude = 12f;
    public float flyingAltitude;
    
    public float angleTarget;
    public float radiusTarget;
    public Transform circleCenter;

    public float turnRightAngle = 180;
    public float turnRightInitAngle;
    public float turnRightOffset = 15f;
    public bool setUpTransitionTurnRight;

    public float flyingAroundTimer;
    public enum DragonStatus {
        Init,
        Waiting,
        FlyingAroundTargetCcw,
        FlyingAroundTargetCw,
        TransitionTurnRight,
        FlyingStraight,
        Chasing,
        Attack,
        Hitting,
        Dead,
    }

    public DragonStatus status = DragonStatus.Init;
    
    public Vector3 startPos, endPos, spawnPosition;

    private void Awake() {
#if UNITY_EDITOR
        UnityEditor.SceneView.FocusWindowIfItsOpen(typeof(UnityEditor.SceneView));
#endif
        if (maxFlyingAltitude < minFlyingAltitude) throw new Exception("Min altitude cannot be greater than max altitude. Using default altitude");
    }

    public void Init(DragonLevel dragonLevel, float f, Vector3 spawnPosition) {
        level = dragonLevel;
        speed = f;
        this.spawnPosition = spawnPosition;
    }

    private void Start() {
        startPos = transform.position;
        SetFlyAroundTimer();
        setUpTransitionTurnRight = true;
    }

    private void Update() {

        if (status == DragonStatus.FlyingAroundTargetCcw || status == DragonStatus.FlyingAroundTargetCw) {
            flyingAroundTimer -= Time.deltaTime;
            if (flyingAroundTimer <= 0) {
                setUpTransitionTurnRight = true;
                status = DragonStatus.TransitionTurnRight;
            }
        }
        
        if (status == DragonStatus.FlyingAroundTargetCcw) {
            circleCenter.position = level.Player.position;
            MoveTransformCircle(false);
        }

        if (status == DragonStatus.FlyingAroundTargetCw) {
            circleCenter.position = level.Player.position;
            MoveTransformCircle(true);
        }

        if (status == DragonStatus.FlyingStraight) {
            if (!HasArrivedToEndPos()) {
                MoveTransformForward();
            }
        }

        if (status == DragonStatus.TransitionTurnRight) {
            if (setUpTransitionTurnRight) {
                circleCenter.position = transform.position + transform.right * turnRightOffset;
                UpdateAngleFrom();
                turnRightInitAngle = angleTarget; 
                turnRightAngle = turnRightInitAngle - 180;
                if (turnRightAngle < 0) turnRightAngle += 360;
            }

            setUpTransitionTurnRight = false;
            MoveTransformCircle(true);
            
            if (angleTarget > turnRightAngle - 2f && angleTarget < turnRightAngle + 2f) {
                SetFlyAroundTimer();
                Vector3 heading = level.Player.position - transform.position;
                float dirNum = AngleDir(transform.forward, heading, transform.up);
                if (dirNum < 0) {
                    status = DragonStatus.FlyingAroundTargetCcw;
                }
                else {
                    status = DragonStatus.FlyingAroundTargetCw;
                }
            }
        }
    }
    
    private void MoveTransformForward() {
        // float walkMultiplier = 1;
        // float distFromStartPos = Vector3.Distance(transform.position, startPos);
        // if (distFromStartPos < 1) walkMultiplier = distFromStartPos; // If we just started ramp up the speed
        //
        // float distToEndPos = Vector3.Distance(transform.position, endPos);
        // if (distToEndPos < 2f) walkMultiplier = distToEndPos * .5f; // If we are close to the target, slow down the speed
        // if (walkMultiplier == 0) walkMultiplier = 1;
        // // anim.speed = walkMultiplier;

        transform.SetPositionAndRotation(
            Vector3.MoveTowards(transform.position, endPos, speed * Time.deltaTime),
            Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(endPos - transform.position),
                2f * Time.deltaTime));
    }

    private void MoveTransformCircle(bool isCw) {
        UpdateAngleFrom();
        angleTarget += Time.deltaTime * speed * (isCw ? -1 : 1);
        if (angleTarget < 0) angleTarget += 360;

        endPos = circleCenter.position + ComputePositionOffset(angleTarget);
        endPos.y = transform.position.y;
        MoveTransformForward();
    }

    private void UpdateAngleFrom() {
        Vector3 sameHeightCenter = circleCenter.position;
        sameHeightCenter.y = transform.position.y;
        Vector3 targetDir = transform.position - sameHeightCenter;
        angleTarget = Vector3.SignedAngle(targetDir, Vector3.right, Vector3.up);
        if (angleTarget < 0) angleTarget += 360;
    }

    private Vector3 ComputePositionOffset(float angle) {
        UpdateRadiusTarget();
        angle *= Mathf.Deg2Rad;
        
        Vector3 positionOffset = new Vector3(
            Mathf.Cos(angle) * radiusTarget,
            0,
            Mathf.Sin(angle) * radiusTarget
            );

        return positionOffset;
    }

    private void UpdateRadiusTarget() {
        float x = transform.position.x - circleCenter.position.x;
        float z = transform.position.z - circleCenter.position.z;
        radiusTarget = Mathf.Sqrt(x * x + z * z);
    }
    
    private float AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up) {
        Vector3 perp = Vector3.Cross(fwd, targetDir);
        float dir = Vector3.Dot(perp, up);
		
        if (dir > 0f) {
            return 1f;
        }

        if (dir < 0f) {
            return -1f;
        }

        return 0f;
    }

    private bool HasArrivedToEndPos() => Vector3.Distance(transform.position, endPos) < 1;
    
    private void SetFlyAroundTimer() => flyingAroundTimer = Random.Range(20, 50);
    
/*#if UNITY_EDITOR
 
    [SerializeField]
    private bool drawGizmos = true;
 
    private void OnDrawGizmosSelected()
    {
        if ( !drawGizmos )
            return;
 
        // Draw an arc around the target
        Vector3 position = Target != null ? Target.position : Vector3.zero;
        Vector3 normal = Vector3.up;
        Vector3 forward = Vector3.forward;
        Vector3 labelPosition;
 
        Vector3 positionOffset = ComputePositionOffset( StartAngle );
        Vector3 verticalOffset;
 
 
        if ( Target != null && UseTargetCoordinateSystem )
        {
            normal = Target.up;
            forward = Target.forward;
        }
        verticalOffset = positionOffset.y * normal;
 
        // Draw label to indicate elevation
        if( Mathf.Abs( positionOffset.y ) > 0.1 )
        {
            UnityEditor.Handles.DrawDottedLine( position, position + verticalOffset, 5 );
            labelPosition = position + verticalOffset * 0.5f;
            labelPosition += Vector3.Cross( verticalOffset.normalized, Target != null && UseTargetCoordinateSystem ? Target.forward : Vector3.forward ) * 0.25f;
            UnityEditor.Handles.Label( labelPosition, ElevationOffset.ToString( "0.00" ) );
        }
 
        position += verticalOffset;
        positionOffset -= verticalOffset;
 
        UnityEditor.Handles.DrawWireArc( position, normal, forward, 360, CircleRadius );
 
        // Draw label to indicate radius
        UnityEditor.Handles.DrawLine( position, position + positionOffset );
        labelPosition = position + positionOffset * 0.5f;
        labelPosition += Vector3.Cross( positionOffset.normalized, Target != null && UseTargetCoordinateSystem ? Target.up : Vector3.up ) * 0.25f;
        UnityEditor.Handles.Label( labelPosition, CircleRadius.ToString( "0.00" ) );
    }
 
#endif*/
}
