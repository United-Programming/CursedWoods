using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour {

  public Transform player;
  public GameObject ArrowPlayer;
  public Transform ArrowPrefab;
  public Animator anim;
  public Camera cam;
  public Terrain Ground;
  public AudioSource audioBow;
  public AudioSource audioGlobal;
  public AudioClip WindDanceMusic;
  public AudioClip Crickets;
  public AudioClip BowDraw;
  public AudioClip ThrowArrow;
  public CursorPointer cursorPointer;

  public int numLives = 3;
  public Image[] Lives;
  public GameObject GameOver;
  public TextMeshProUGUI LevelProgress;

  public float playerTargetAngle = 0; // public just to debug
  public float movement = 0; // public just to debug
  public float angle = 0; // public just to debug
  public float speed = .05f; // public just to debug
  public bool aiming = false; // public just to debug
  public bool dead = false; // public just to debug
  public bool win = false; // public just to debug

  Level level;
  public Level[] Levels;

  private IEnumerator Start() {
    yield return new WaitForSeconds(.2f);
    Ground = GameObject.FindObjectOfType<Terrain>(); // FIXME, remove it when the scenes will be merged

    for (int i = 0; i < Lives.Length; i++) {
      Lives[i].enabled = i < numLives;
    }

    level = Levels[1]; // FIXME
    level.Init(Ground, this);
    LevelProgress.text = $"{level.GetName()}\n0/{level.GetToWin()}";
  }

  private void Update() {
    if (win) {
      if ((audioGlobal.clip == WindDanceMusic && !audioGlobal.isPlaying) || Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonUp(0)) {
        win = false;
        dead = false;
        anim.SetBool("Aim", false);
        anim.Play("Idle");
        audioGlobal.clip = Crickets;
        audioGlobal.loop = true;
        audioGlobal.Play();
        level.Init(Ground, this);
      }
      return;
    }
    if (dead) return;


    if (Input.GetKeyDown(KeyCode.P)) PlayWinDance();

    if (Input.GetMouseButtonDown(1)) { // Change aiming/no-aiming if we press the right mouse buton
      arrowLoaded = false;
      cursorPointer.aimingCursor = false;
      aiming = !aiming;
      anim.SetBool("Aim", aiming);
      if (!aiming) ArrowPlayer.SetActive(false);
      else {
        audioBow.clip = BowDraw;
        audioBow.Play();
      }
    }

    float x = Input.GetAxis("Horizontal");
    if (x != 0) { // If we move we cannto be aiming
      aiming = false;
      arrowLoaded = false;
      cursorPointer.aimingCursor = false;
      anim.SetBool("Aim", false);
      ArrowPlayer.SetActive(false);
    }

    if (aiming && Input.GetMouseButtonDown(0) && arrowLoaded) { // We should eb sure the aiming anim is completed
      audioBow.clip = ThrowArrow;
      audioBow.Play();
      anim.Play("Shoot");
    }


    if (x == 0 || aiming) { // not moving
      // just keep the player angle and stop the run anim, but do not change the player rotation
      // Stop the rotation of the camera
      anim.SetBool("Moving", false);
      anim.SetBool("Run", false);
      movement = 0;
      angle = transform.rotation.eulerAngles.y;
      anim.speed = 1;
    }
    else { // Moving, change first player rotation, then move the camera
      // Align the local rotation of the player to the movement
      playerTargetAngle = Mathf.LerpAngle(playerTargetAngle, x > 0 ? 90 : -90, 6 * Time.deltaTime);
      float playerCurrentAngle = player.localEulerAngles.y;
      float dist = Mathf.Abs(playerTargetAngle - playerCurrentAngle);
      player.localRotation = Quaternion.Euler(0, Mathf.Lerp(playerCurrentAngle, playerTargetAngle, dist * 10 * Time.deltaTime), 0);

      // Depending on the angle magnitude, set the movement anim
      float absPAngle = Mathf.Abs(playerTargetAngle);
      anim.SetBool("Moving", absPAngle > 5);
      
      if (absPAngle > 85) {
        anim.SetBool("Run", true);
        float mult = Mathf.Sign(movement) == Mathf.Sign(x) ? 1.5f : 10f;
        anim.speed = .25f + Mathf.Abs(movement);
        movement += x * Time.deltaTime * mult;
        movement = Mathf.Clamp(movement, -1f, 1f);

        // Notate the camera and the player around the world
        angle += movement * speed;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, angle, 0), 6 * Time.deltaTime);
      }
      else anim.SetBool("Run", false);
    }

    if (aiming) {
      float aimH = 2f * (Input.mousePosition.x - Screen.width * .5f) / Screen.width - .09f;
      aimV = 2f * (Input.mousePosition.y - Screen.height * .5f) / Screen.height;
      anim.SetFloat("AimH", aimH);
      anim.SetFloat("AimV", aimV);

      // When aiming, the local rotation of the player should be looking to the center, and move pretty quick
      playerTargetAngle = Mathf.LerpAngle(playerTargetAngle, aimH * 45, 8 * Time.deltaTime);
      float playerCurrentAngle = player.localEulerAngles.y;
      float dist = Mathf.Abs(playerTargetAngle - playerCurrentAngle);
      player.localRotation = Quaternion.Euler(0, Mathf.Lerp(playerCurrentAngle, playerTargetAngle, dist * 15 * Time.deltaTime), 0);


      if (arrowLoaded) {
        Vector3 start = HandRefR.position;
        Vector3 dest = start + (HandRefL.position - HandRefR.position + Vector3.up * .01f).normalized * 5;
        cursorPointer.cursorPosition = dest;
        start.y = Ground.SampleHeight(start);
        dest.y = Ground.SampleHeight(dest);
        Debug.DrawLine(start, dest, Color.red);
      }
    }
  }

  public void PlayerDeath() {
    ArrowPlayer.SetActive(false);
    if (Random.Range(0, 2) == 0) anim.Play("Death1");
    else anim.Play("Death2");
    dead = true;

    // Reduce the numebr of lives.
    numLives--;
    if (numLives == 0) { // If -1 then show the game over
      GameOver.SetActive(true);
      return;
    }
    // If not, pause for a while, update UI, and restart the level
    for (int i = 0; i < Lives.Length; i++) {
      Lives[i].enabled = i < numLives;
    }
    StartCoroutine(RestartLevel());
  }

  IEnumerator RestartLevel() {
    yield return new WaitForSeconds(4);
    dead = false;
    anim.Play("Idle");
    level.Init(Ground, this);
  }

  public void PlayWinDance() {
    ArrowPlayer.SetActive(false);
    win = true;
    anim.Play("WinDance");
    audioGlobal.clip = WindDanceMusic;
    audioGlobal.loop = false;
    audioGlobal.Play();
    // Wait for mouse press or key press or end of anim
  }

  public Transform HandRefR, HandRefL;
  public float aimV;

  private void OnApplicationFocus(bool focus) {
    Cursor.visible = !focus;
  }

  public bool arrowLoaded = false;
  public float arrowforce = 100;

  public void ArrowLoaded() {
    arrowLoaded = true;
    cursorPointer.aimingCursor = true;
  }

  public void ArrowShoot() {
    if (!arrowLoaded) return;
    arrowLoaded = false;
    cursorPointer.aimingCursor = false;
    audioBow.clip = BowDraw;
    audioBow.Play();
    if (Instantiate(ArrowPrefab).GetChild(0).TryGetComponent(out Arrow arrow)) {
      Vector3 start = HandRefR.position;
      Vector3 dir = (HandRefL.position - HandRefR.position + Vector3.up * .01f).normalized;
      Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
      float extraForce = 1;
      if (aimV >= 0) extraForce = -4 * aimV * aimV + 4 * aimV + 1;
      arrow.Init(start, rot, arrowforce * extraForce * dir, Ground);
    }
  }

  internal void EnemyKilled(int done, int toWin) {
    LevelProgress.text = $"{level.GetName()}\n{done}/{level.GetToWin()}";
  }
}