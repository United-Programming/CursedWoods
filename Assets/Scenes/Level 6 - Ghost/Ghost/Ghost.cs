using Unity.VisualScripting;
using UnityEngine;

public class Ghost : MonoBehaviour {
  public Level6 level;
  public Animator anim;
  public Transform BodyCenter;
  public LayerMask ArrowMask, PlayerMask;
  public AudioSource sounds;
  public AudioClip WalkSounds;
  public AudioClip AttackSound;
  public AudioClip DeathSound;
  public AudioClip HitSound;
  public SkinnedMeshRenderer skinnedMeshRenderer;
  Material ghostMat, ghostBMat;

  public enum GhostStatus {
    Waiting, Walking, RunToOrb, InterceptPlayer, Attack, Hit, Dead
  };
  public GhostStatus status = GhostStatus.Waiting;


  internal void Init(Level6 l) {
    ghostMat = skinnedMeshRenderer.materials[0];
    ghostBMat = skinnedMeshRenderer.materials[1];
    anim.Play("Idle");
    status = GhostStatus.Walking;
    Controller.Dbg(status.ToString());
    level = l;
    center = l.GetLevelCenter();

    targetAngle = Random.Range(-360f, 360f);
    targetDistance = Random.Range(35f, 42f);
    targetAlpha = Random.Range(.25f, 1f);
    angle = targetAngle;
    distance = targetDistance;
    alpha = targetAlpha;
    Vector3 pos = center;
    pos.x += Mathf.Sin(targetAngle * Mathf.Deg2Rad) * targetDistance;
    pos.z += Mathf.Cos(targetAngle * Mathf.Deg2Rad) * targetDistance;
    pos.y = level.Forest.SampleHeight(pos);
    transform.position = pos;
    sounds.clip = WalkSounds;
    sounds.loop = true;
    sounds.Play();
  }

  public float angle, centerdistance;
  float distance, targetAngle, targetDistance, alpha, targetAlpha;
  float acceleration;
  Vector3 center;



  private void Update() {
    if (level == null) return;

    centerdistance = Vector3.Distance(center, transform.position);
    

    if (status == GhostStatus.RunToOrb) {
      float dist = Vector3.Distance(targetPosition, transform.position);
      transform.position = Vector3.MoveTowards(transform.position, targetPosition, Mathf.Clamp(dist, 2, 25) * Time.deltaTime);

      Vector3 pos = transform.position;
      pos.y = level.Forest.SampleHeight(pos);
      transform.position = pos;

      if (dist < 10) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(level.Player.position - transform.position), Time.deltaTime);
      else transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetPosition - transform.position), Time.deltaTime);
      if (dist < 2) {
        interceptTime = 0;
        status = GhostStatus.InterceptPlayer;
        Controller.Dbg(status.ToString());
      }
    }

    if (status == GhostStatus.Walking) {
      if (acceleration < Mathf.Clamp(Mathf.Abs(angle - targetAngle) * .1f, .1f, 25)) acceleration += Time.deltaTime * 1.5f;

      if (Mathf.Abs(angle - targetAngle) < 1) {
        targetAngle = Random.Range(-360f, 360f);
        targetDistance = Random.Range(35f, 42f);
        acceleration = 0;
      }
      if (angle < targetAngle) angle += Time.deltaTime * acceleration;
      else angle -= Time.deltaTime * acceleration;

      if (Mathf.Abs(distance - targetDistance) < .1f) targetDistance = Random.Range(32f, 45f);
      if (distance < targetDistance) distance += Time.deltaTime * .5f;
      else distance -= Time.deltaTime * .5f;

      if (Mathf.Abs(alpha - targetAlpha) < .01f) targetAlpha = Random.Range(.25f, 1f);
      if (alpha < targetAlpha) alpha += Time.deltaTime * .1f;
      else alpha -= Time.deltaTime * .1f;

      Vector3 pos = center;
      pos.x += Mathf.Sin(angle * Mathf.Deg2Rad) * distance;
      pos.z += Mathf.Cos(angle * Mathf.Deg2Rad) * distance;
      pos.y = level.Forest.SampleHeight(pos);

      transform.SetPositionAndRotation(
        Vector3.Slerp(transform.position, pos, Time.deltaTime * acceleration), 
        Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(pos - transform.position), Time.deltaTime));

      ghostMat.SetFloat("_Alpha", alpha);
      ghostBMat.SetFloat("_Alpha", alpha);

      // Are we on about the same line of the player?
      float ghostAngle = Vector3.SignedAngle(level.Player.position - center, transform.position - center, Vector3.up);
      if (Mathf.Abs(ghostAngle) < 8 && Vector3.Distance(transform.position, center) > 23 && skinnedMeshRenderer.isVisible) {
        status = GhostStatus.InterceptPlayer;
        Controller.Dbg(status.ToString());
        interceptTime = 0;
        targetAngle = angle;
      }
    }


    if (status == GhostStatus.InterceptPlayer) {
      // Make alpha fully visible
      if (alpha < 1) {
        alpha += Time.deltaTime * 5;
        if (alpha > 1) alpha = 1;
        ghostMat.SetFloat("_Alpha", alpha);
        ghostBMat.SetFloat("_Alpha", alpha);
      }

      // Rotate to the player in 1 second
      Quaternion look = Quaternion.LookRotation(level.Player.position - transform.position);
      transform.rotation = Quaternion.Slerp(transform.rotation, look, 2 * Time.deltaTime);

      // Go closer to the player position
      Vector3 axis = transform.position - center;
      transform.position = Vector3.MoveTowards(transform.position, center + 29 * axis.normalized, 5 * Time.deltaTime);

      interceptTime += Time.deltaTime;
      if (interceptTime < .5f) return;

      // When all values are good start screaming attack, but check also if the player went away
      float lookAngle = Quaternion.Angle(transform.rotation, look);
      if (alpha == 1 && lookAngle < 5 && (transform.position - center).magnitude > 27 && skinnedMeshRenderer.isVisible) {
        status = GhostStatus.Attack;
        Controller.Dbg(status.ToString());
        anim.Play("Attack");
        sounds.clip = AttackSound;
        sounds.loop = false;
        sounds.Play();
      }

      if (interceptTime < 2) return;
      // Check if the player went away
      if (lookAngle > 24) {
        status = GhostStatus.Walking;
        Controller.Dbg(status.ToString());
      }
    }

    if (status == GhostStatus.Hit) {
      // Block for a while
      stopTimeout -= Time.deltaTime;
      if (stopTimeout < 0) {
        targetAngle = Random.Range(-360f, 360f);
        targetDistance = Random.Range(35f, 42f);
        targetAlpha = Random.Range(.25f, 1f);
        status = GhostStatus.Walking;
        Controller.Dbg(status.ToString());
      }
    }
  }
  float interceptTime = 0;


  public void AttackCompleted() {
    status = GhostStatus.Walking;
    Controller.Dbg(status.ToString());
    sounds.clip = WalkSounds;
    sounds.loop = true;
    sounds.Play();
  }
  public void AttackStarted() {
    // Check distance and angle
    float ghostAngle = Vector3.SignedAngle(level.Player.position - center, transform.position - center, Vector3.up);
    if (Mathf.Abs(ghostAngle) > 24 || !skinnedMeshRenderer.isVisible) {
      status = GhostStatus.Walking;
      Controller.Dbg(status.ToString());
      sounds.clip = WalkSounds;
      sounds.loop = true;
      sounds.Play();
      return;
    }

    // Wait till the end of anim or the end of the sound, then restart walking
    level.Game.ShakeCamera(.5f);
    level.PlayerDeath();
  }



  public void TakeDamage() {
    Debug.Log("Damaged orb!");
    status = GhostStatus.Hit;
    Controller.Dbg(status.ToString());
    // Emit sound
    sounds.clip = HitSound;
    sounds.loop = false;
    sounds.Play();
    // Decrease health, if goes to zero kill enemy for good
    level.KillEnemy(gameObject);

    anim.Play("Death"); // Play some pre-death anim
    if (!level.Completed) stopTimeout = 4; // Block for 4 seconds
  }

  Vector3 targetPosition;
  float stopTimeout;

  internal void ReachOrb(Vector3 orbPosition) {
    if (status != GhostStatus.Walking) return;

    targetPosition = orbPosition + Random.insideUnitSphere * 3;
    targetPosition.y = level.Forest.SampleHeight(targetPosition) + .2f;
    status = GhostStatus.RunToOrb;
    Controller.Dbg(status.ToString());
  }

  internal void Call(Vector3 orbPos) {
    Vector3 uni = (orbPos - center);
    targetDistance = uni.magnitude;
    uni.Normalize();
    targetAngle = Mathf.Atan2(uni.x, uni.z) * Mathf.Rad2Deg;
  }
}
