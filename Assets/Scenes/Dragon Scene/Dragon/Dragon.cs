using System;
using System.Collections;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class Dragon : MonoBehaviour {
    /*
     *  TODO: Evaluate some logic for the init state to the flying status (distance to center, angle of turning, etc.)
     */

    public DragonLevel level;
    public Animator anim;

    public LayerMask arrow;
    public GameObject arrowPrefab;
    public DragonEyeCollider dragonHitPlace;
    public DragonAnimationEvents dragonAnimationEvents;

    public float speed = 5f;
    public float flyingAwayMultiplier = 2f;
    public float minFlyingAltitude; // make the wings won't clip with the terrain.

    [Header("Flying Section")] public float angleTarget;
    public float radiusTarget;
    public Transform circleCenter;

    public float transitionTurnAngle;
    public float transitionTurnOffset = 15f;
    public TurnDirection transitionTurnDirection;

    public float flyingAroundTimer;
    public float flyingMinTime = 10f;
    public float flyingMaxTime = 25f;

    [Header("Flying Wings Effect Section")]
    public float wingsEffectHeightDelta = 5f;

    public float wingsEffectUpDuration = .5f;
    public float wingsEffectUpTimer;
    public float wingsEffectDownDuration = 1.5f;
    public float wingsEffectDownTimer;
    public bool applyWingUpEffect;
    public bool applyWingDownEffect;

    [Header("Attacking Section")] public float attackingTimer;
    public float attackingMinTime = 5f;
    public float attackingMaxTime = 10f;
    public Fireball fireballPrefab;
    public Transform fireballStartLocation;
    public int fireballAmount = 3;
    public float fireballIntervalTime = .5f;
    public int fireballShoot = 0;
    public float lastShootTime;

    [Header("Hit Status Section")] public int dragonHitPoints = 3;
    public bool hasBeenHit;
    public GameObject eyesGameObject;
    public Material normalEyeMaterial;
    public Material hitEyeMaterial;
    public float hitStatusTime = 5f;
    public float hitStatusTimer;
    public Vector3 playerPositionOnDead;

    [Header("Debugging")] public bool debugHitPoint;

    public enum DragonStatus {
        Init,
        Waiting,
        FlyingAroundTargetCcw,
        FlyingAroundTargetCw,
        TransitionTurn,
        FlyingStraight,
        FlyingAway,
        Dead,
    }

    public DragonStatus status = DragonStatus.Init;

    private Vector3 startPos, endPos, spawnPosition;

    public enum TurnDirection {
        Right,
        Left,
        None,
    }

    public void Init(DragonLevel dragonLevel, float f, Vector3 spawnPosition) {
        level = dragonLevel;
        speed = f;
        this.spawnPosition = spawnPosition;
        dragonAnimationEvents.SetDragon(this);
    }

    private void Start() {
        Init(level, 15, transform.position);
        startPos = transform.position;
        SetFlyAroundTimer();
        SetHitStatusTimer();
    }

    private void Update() {
        if (status == DragonStatus.Init) return;

        if (debugHitPoint) {
            debugHitPoint = false;
            if (arrowPrefab == null || dragonHitPlace == null) return;
            StartCoroutine(DebugHitDragon());
        }

        if (status is DragonStatus.FlyingAroundTargetCcw or DragonStatus.FlyingAroundTargetCw) {
            flyingAroundTimer -= Time.deltaTime;
            if (flyingAroundTimer <= 0) {
                status = DragonStatus.TransitionTurn;
                transitionTurnDirection = TurnDirection.Right;
                SetUpTurn(180);
            }
        }

        if (status == DragonStatus.FlyingAroundTargetCcw) {
            circleCenter.position = level.Player.position;
            MoveInCircles(TurnDirection.Left);
        }

        if (status == DragonStatus.FlyingAroundTargetCw) {
            circleCenter.position = level.Player.position;
            MoveInCircles(TurnDirection.Right);
        }

        if (status == DragonStatus.FlyingStraight) {
            if (!HasArrivedToEndPos()) {
                MoveForward(TurnDirection.None);
            }
        }

        if (status == DragonStatus.FlyingAway) {
            if (!HasArrivedToEndPos()) {
                MoveForward(TurnDirection.None);
            }
            else {
                Destroy(gameObject);
            }
        }

        if (status == DragonStatus.TransitionTurn) {
            TurnTransitionUpdate();
        }

        if (status == DragonStatus.Dead) {
            MoveInCircles(transitionTurnDirection);
            float angleToTarget = GetPlayerPositionAngle();
            if (angleToTarget is < 1f and > -1f) {
                endPos = transform.position + transform.forward * 500f;
                status = DragonStatus.FlyingAway;
            }
        }

        if (status is DragonStatus.FlyingAroundTargetCcw or DragonStatus.FlyingAroundTargetCw) {
            ShootFireballUpdate();
        }

        UpdateWingUpEffectTimer();
        UpdateWingDownEffectTimer();

        if (hasBeenHit && status is not (DragonStatus.Dead or DragonStatus.FlyingAway)) {
            CountDownHitStatusTimer();
        }
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

    private void TurnTransitionUpdate() {
        MoveInCircles(transitionTurnDirection);
        if (angleTarget > transitionTurnAngle - 2f && angleTarget < transitionTurnAngle + 2f) {
            SetFlyAroundTimer();
            status = AngleDir() < 0 ? DragonStatus.FlyingAroundTargetCcw : DragonStatus.FlyingAroundTargetCw;

            anim.speed = 1;
        }
    }

    private void SetUpTurn(float angle) {
        Vector3 sideOffset = transitionTurnDirection == TurnDirection.Right ? transform.right : transform.right * -1;
        circleCenter.position = transform.position + sideOffset * transitionTurnOffset;
        UpdateNextAngleTarget();
        transitionTurnAngle = angleTarget - angle;
        if (transitionTurnAngle < 0) transitionTurnAngle += 360;
    }

    private void MoveForward(TurnDirection turnDirection) {
        endPos.y = CalculateNextPositionYAxis();

        float targetRotationZEuler = RotateZAxis(turnDirection);
        Quaternion targetRotation = Quaternion.LookRotation(endPos - transform.position);
        float targetRotationY = Mathf.LerpAngle(transform.rotation.eulerAngles.y, targetRotation.eulerAngles.y,
            2f * Time.deltaTime);
        Quaternion nextRotation = Quaternion.Euler(0, targetRotationY, targetRotationZEuler);

        Vector3 nextPosition = Vector3.MoveTowards(transform.position, endPos, speed * Time.deltaTime);

        transform.SetPositionAndRotation(nextPosition, nextRotation);
    }

    private float CalculateNextPositionYAxis() {
        float baseHeight = level.Forest.SampleHeight(endPos) + minFlyingAltitude;

        if (applyWingUpEffect) {
            return Mathf.Lerp(baseHeight, baseHeight + wingsEffectHeightDelta,
                wingsEffectUpTimer / wingsEffectUpDuration);
        }

        if (applyWingDownEffect) {
            return Mathf.Lerp(baseHeight + wingsEffectHeightDelta, baseHeight,
                wingsEffectDownTimer / wingsEffectDownDuration);
        }

        return baseHeight;
    }

    private void MoveInCircles(TurnDirection turnDirection) {
        UpdateNextAngleTarget();
        float angleDirectionMultiplier = 0;
        if (turnDirection == TurnDirection.Right) angleDirectionMultiplier = -1f;
        if (turnDirection == TurnDirection.Left) angleDirectionMultiplier = 1f;
        
        angleTarget += Time.deltaTime * speed * angleDirectionMultiplier;
        if (angleTarget < 0) angleTarget += 360;

        endPos = circleCenter.position + ComputePositionOffset(angleTarget);
        endPos.y = transform.position.y;

        MoveForward(turnDirection);
    }

    private float RotateZAxis(TurnDirection turnDirection) {
        float targetAngle = 0;
        if (turnDirection == TurnDirection.Right) {
            targetAngle = -25f;
        }

        if (turnDirection == TurnDirection.Left) {
            targetAngle = 25f;
        }

        if (turnDirection == TurnDirection.Right && transform.rotation.eulerAngles.z is > 335 - 1f and <= 335 + 1f)
            return targetAngle;
        if (turnDirection == TurnDirection.Left && transform.rotation.eulerAngles.z is >= 25f - 1f and <= 25 + 1f)
            return targetAngle;
        if (turnDirection == TurnDirection.None && transform.rotation.eulerAngles.z is > -1f and <= 1f)
            return targetAngle;

        float signedZRotationEuler = (transform.rotation.eulerAngles.z + 180f) % 360f - 180f;
        float nextZRotationEuler = Mathf.Lerp(signedZRotationEuler, targetAngle, Time.deltaTime);

        return nextZRotationEuler;
    }

    private void UpdateNextAngleTarget() {
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

    private float AngleDir() {
        Vector3 targetDir = level.Player.position - transform.position;
        Vector3 perp = Vector3.Cross(transform.forward, targetDir);
        float dir = Vector3.Dot(perp, transform.up);

        if (dir > 0f) {
            return 1f;
        }

        if (dir < 0f) {
            return -1f;
        }

        return 0f;
    }

    public void EyeShoot() {
        if (hasBeenHit) return;
        if (status is DragonStatus.Dead or DragonStatus.Init) return;

        dragonHitPoints--;
        hasBeenHit = true;
        eyesGameObject.GetComponent<SkinnedMeshRenderer>().material = hitEyeMaterial;
        CheckIfDragonIsDead();
    }

    private void CountDownHitStatusTimer() {
        hitStatusTimer -= Time.deltaTime;

        if (hitStatusTimer <= 0f) {
            ResetEyeHitStatus();
        }
    }

    private void ResetEyeHitStatus() {
        eyesGameObject.GetComponent<SkinnedMeshRenderer>().material = normalEyeMaterial;
        hasBeenHit = false;
        SetHitStatusTimer();
    }

    private void CheckIfDragonIsDead() {
        if (dragonHitPoints <= 0) {
            SetUpDeadStatus();
        }
    }

    private void SetUpDeadStatus() {
        status = DragonStatus.Dead;
        transitionTurnDirection = AngleDir() > 0 ? TurnDirection.Right : TurnDirection.Left;
        playerPositionOnDead = level.Player.position;
        SetUpTurn(360);
        // TODO lerp it to get a smooth transition (not here on the flyingaway status).
        speed *= flyingAwayMultiplier;
        anim.speed *= flyingAwayMultiplier;
    }

    private float GetPlayerPositionAngle() {
        Vector3 playerVector = playerPositionOnDead - transform.position;
        return Vector2.SignedAngle(
            new Vector2(transform.forward.x, transform.forward.z),
            new Vector2(playerVector.x, playerVector.z)
        );
    }

    private bool HasArrivedToEndPos() => Vector3.Distance(transform.position, endPos) < 1;

    private void SetFlyAroundTimer() => flyingAroundTimer = Random.Range(flyingMinTime, flyingMaxTime);
    private void SetHitStatusTimer() => hitStatusTimer = Random.Range(hitStatusTime - 1f, hitStatusTime + 1f);
    
    public LayerMask GetArrowLayerMask() => arrow;

    public void MovingWingsDown() {
        if (status == DragonStatus.Init) return;
        applyWingUpEffect = true;
    }

    private void UpdateWingUpEffectTimer() {
        if (!applyWingUpEffect) return;

        wingsEffectUpTimer += Time.deltaTime;
        if (wingsEffectUpTimer >= wingsEffectUpDuration) {
            wingsEffectUpTimer = 0f;
            applyWingUpEffect = false;
            applyWingDownEffect = true;
        }
    }

    private void UpdateWingDownEffectTimer() {
        if (!applyWingDownEffect) return;
        wingsEffectDownTimer += Time.deltaTime;
        if (wingsEffectDownTimer >= wingsEffectDownDuration) {
            applyWingDownEffect = false;
            wingsEffectDownTimer = 0f;
        }
    }

    public void ReachGlideWingPosition() {
        if (status == DragonStatus.TransitionTurn) {
            anim.speed = 0.5f;
        }
    }

    private IEnumerator DebugHitDragon() {
        float elapsedTime = 0;
        GameObject arrowGameObject = Instantiate(arrowPrefab, dragonHitPlace.transform.position, Quaternion.identity);
        while (elapsedTime <= .5f) {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(arrowGameObject);
    }
}