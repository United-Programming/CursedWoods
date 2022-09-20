using System;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

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
    public float turnRightOffset = 15f;

    public float flyingAroundTimer;
    public float flyingMinTime = 10f;
    public float flyingMaxTime = 25f;

    public float attackingTimer;
    public float attackingMinTime = 5f;
    public float attackingMaxTime = 10f;
    public Fireball fireballPrefab;
    public Transform fireballStartLocation;
    public int fireballAmount = 3;
    public float fireballIntervalTime = .5f;
    public int fireballShoot = 0;
    public float lastShootTime;

    // TODO: Collider on eyes, get ontrigger is an arrow it count as a hit.
    // TODO: Death when the dragon is killed it will fly away and ¿disappear?
    // TODO: Put events on flying anim: 2 for moving transform up and down. another for the static anim when transitioning.
    // TODO: Evaluate some logic for the init state to the flying status (distance to center, angle of turning, etc.)
    
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
        if (maxFlyingAltitude < minFlyingAltitude)
            throw new Exception("Min altitude cannot be greater than max altitude. Using default altitude");
    }

    public void Init(DragonLevel dragonLevel, float f, Vector3 spawnPosition) {
        level = dragonLevel;
        speed = f;
        this.spawnPosition = spawnPosition;
    }

    private void Start() {
        startPos = transform.position;
        SetFlyAroundTimer();
    }

    private void Update() {
        if (status == DragonStatus.Init) return;
        
        if (status is DragonStatus.FlyingAroundTargetCcw or DragonStatus.FlyingAroundTargetCw) {
            flyingAroundTimer -= Time.deltaTime;
            if (flyingAroundTimer <= 0) {
                status = DragonStatus.TransitionTurnRight;
                SetUpTurnRight();
            }
        }

        if (status == DragonStatus.FlyingAroundTargetCcw) {
            circleCenter.position = level.Player.position;
            MoveInCircles(false);
        }

        if (status == DragonStatus.FlyingAroundTargetCw) {
            circleCenter.position = level.Player.position;
            MoveInCircles(true);
        }

        if (status == DragonStatus.FlyingStraight) {
            if (!HasArrivedToEndPos()) {
                MoveTransformForward();
            }
        }

        if (status == DragonStatus.TransitionTurnRight) {
            TurnRightTransitionUpdate();
        }

        ShootFireballUpdate();
    }

    private void ShootFireballUpdate() {
        if (attackingTimer <= 0f) {
            if (fireballShoot == 0 || attackingTimer <= lastShootTime - fireballIntervalTime) {
                lastShootTime = attackingTimer;
                Fireball fireball = Instantiate(fireballPrefab, fireballStartLocation.position, Quaternion.identity);
                fireball.SetTarget(level.Player.position);
                fireballShoot++;
            }

            if (fireballShoot >= fireballAmount) {
                attackingTimer = Random.Range(attackingMinTime, attackingMaxTime);
                fireballShoot = 0;
            }
        }

        attackingTimer -= Time.deltaTime;
    }

    private void TurnRightTransitionUpdate() {
        MoveInCircles(true);

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

    private void SetUpTurnRight() {
        circleCenter.position = transform.position + transform.right * turnRightOffset;
        UpdateAngleFrom();
        turnRightAngle = angleTarget - 180;
        if (turnRightAngle < 0) turnRightAngle += 360;
    }

    private void MoveTransformForward(bool isCw = true) {
        // float walkMultiplier = 1;
        // float distFromStartPos = Vector3.Distance(transform.position, startPos);
        // if (distFromStartPos < 1) walkMultiplier = distFromStartPos; // If we just started ramp up the speed
        //
        // float distToEndPos = Vector3.Distance(transform.position, endPos);
        // if (distToEndPos < 2f) walkMultiplier = distToEndPos * .5f; // If we are close to the target, slow down the speed
        // if (walkMultiplier == 0) walkMultiplier = 1;
        // // anim.speed = walkMultiplier;

        float targetRotationZEuler = RotateZAxis(isCw);
        Quaternion targetRotation = Quaternion.LookRotation(endPos - transform.position);
        float targetRotationY = Mathf.LerpAngle(transform.rotation.eulerAngles.y, targetRotation.eulerAngles.y,
            2f * Time.deltaTime);
        Quaternion nextRotation = Quaternion.Euler(0, targetRotationY, targetRotationZEuler);

        Vector3 nextPosition = Vector3.MoveTowards(transform.position, endPos, speed * Time.deltaTime);

        transform.SetPositionAndRotation(nextPosition, nextRotation);
    }

    private void MoveInCircles(bool isCw) {
        UpdateAngleFrom();
        angleTarget += Time.deltaTime * speed * (isCw ? -1 : 1);
        if (angleTarget < 0) angleTarget += 360;

        endPos = circleCenter.position + ComputePositionOffset(angleTarget);
        endPos.y = transform.position.y;

        MoveTransformForward(isCw);
    }

    private float RotateZAxis(bool isCw) {
        if (isCw && transform.rotation.eulerAngles.z is > 335 - 1f and <= 335 + 1f) return -25f;
        if (!isCw && transform.rotation.eulerAngles.z is >= 25f - 1f and <= 25 + 1f) return 25f;

        float targetAngle = isCw ? -25f : 25f;
        float signedAngle = (transform.rotation.eulerAngles.z + 180f) % 360f - 180f;
        float nextAngle = Mathf.Lerp(signedAngle, targetAngle, Time.deltaTime);

        return nextAngle;
    }

    private void UpdateAngleFrom() {
        Vector3 sameHeightCenter = new Vector3(circleCenter.position.x, transform.position.y, circleCenter.position.z);
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

    private void SetFlyAroundTimer() => flyingAroundTimer = Random.Range(flyingMinTime, flyingMaxTime);
}