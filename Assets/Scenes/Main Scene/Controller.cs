using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Controller : MonoBehaviour {

  public Transform player;
  public GameObject ArrowPlayer;
  public Transform ArrowPrefab;
  public Animator anim;
  public Camera cam;
  public Terrain Ground;
  public AudioMixer MasterMixer;
  public AudioSource audioBow;
  public AudioSource audioGlobal;
  public AudioClip WindDanceMusic;
  public AudioClip Crickets;
  public AudioClip BowDraw;
  public AudioClip ThrowArrow;
  public CursorPointer cursorPointer;
  public LineRenderer lineRenderer;

  public int numLives = 3;
  public Image[] Lives;
  public GameObject GameOver;
  public TextMeshProUGUI LevelProgress;
  public TextMeshProUGUI NumOfPlay;
  public TextMeshProUGUI NumOfKillsG;
  public TextMeshProUGUI NumOfKillsT;
  public TextMeshProUGUI NumOfShootsG;
  public TextMeshProUGUI NumOfShootsT;
  public TextMeshProUGUI NumOfAccuracyG;
  public TextMeshProUGUI NumOfAccuracyT;

  float playerTargetAngle = 0;
  float movement = 0;
  float angle = 0;
  public float speed = .18f;

  // no aiming, loading, aiming with arrow ready
  public enum Aiming { NotAiming, Loading, ArrowReady };
  public Aiming aiming = Aiming.NotAiming;

  bool crushed = false;

  int currentLevel;
  Level level;
  public Level[] Levels;


  public enum GameStatus {
    Intro, Play, Pause, WinDance, Death, GameOver
  }
  public GameStatus gameStatus = GameStatus.GameOver;
  [SerializeField] GameObject GamePlay, Intro, GameCanvas, PauseMenu;


  private IEnumerator Start() {
    yield return new WaitForSeconds(.2f);
    if (Ground == null) Ground = GameObject.FindObjectOfType<Terrain>(); // FIXME, remove it when the scenes will be merged
    SetGameStatus(GameStatus.Intro);
  }

  public void StartNewGame() {
    PlayerData.ResetStats();
    PlayerData.PlayAnotherGame();
    SetGameStatus(GameStatus.Play);
  }

  bool GoNextlevel() {
    currentLevel++;
    if (currentLevel >= Levels.Length) {
      LevelProgress.text = $"No more available levels!";
      SetGameStatus(GameStatus.Intro);
      Debug.Log("--------------------------------------- No more available levels!");
      return true;
    }
    if (level != null) level.gameObject.SetActive(false);

// FIXME    currentLevel = 3;

    level = Levels[currentLevel];
    level.gameObject.SetActive(true);
    level.Init(Ground, this, false);
    LevelProgress.text = $"{level.GetName()}\n0/{level.GetToWin()}";
    return false;
  }

  private void SetGameStatus(GameStatus status) {
    if (status == gameStatus) return;

    switch (status) {
      case GameStatus.Intro:
        lineRenderer.enabled = false;
        player.gameObject.SetActive(false);
        GamePlay.SetActive(false);
        GameCanvas.SetActive(false);
        Intro.SetActive(true);
        PauseMenu.SetActive(false);
        Ground.gameObject.SetActive(false);
        GameOver.SetActive(false);
        gameStatus = GameStatus.Intro;
        Cursor.visible = true;
        break;

      case GameStatus.Play:
        Cursor.visible = false;
        lineRenderer.enabled = false;
        player.gameObject.SetActive(true);
        GamePlay.SetActive(true);
        GameCanvas.SetActive(true);
        Intro.SetActive(false);
        PauseMenu.SetActive(false);
        Ground.gameObject.SetActive(true);
        GameOver.SetActive(false);
        if (gameStatus == GameStatus.Pause) {
          PauseMenu.SetActive(false);
          Time.timeScale = 1;
        }
        else if (gameStatus == GameStatus.Death) {
          anim.Play("Idle");
          level.Init(Ground, this, true);
        }
        else if (gameStatus == GameStatus.Intro) {
          StartCoroutine(StartGame());
        }
        gameStatus = GameStatus.Play;
        break;

      case GameStatus.Pause:
        PauseMenu.SetActive(true);
        Time.timeScale = 0;
        Cursor.visible = true;

        NumOfPlay.text = PlayerData.GetNumberPlays().ToString();
        int numKG = PlayerData.GetNumberOfKills(false);
        int numKT = PlayerData.GetNumberOfKills(true);
        int numSG = PlayerData.GetNumberOfShoots(false);
        int numST = PlayerData.GetNumberOfShoots(true);
        NumOfKillsG.text = numKG.ToString();
        NumOfKillsT.text = numKT.ToString();
        NumOfShootsG.text = numSG.ToString();
        NumOfShootsT.text = numST.ToString();
        NumOfAccuracyG.text = (int)((numKG * 1f / (numSG == 0 ? 1 : numSG)) * 100) + "%";
        NumOfAccuracyT.text = (int)((numKT * 1f / (numST == 0 ? 1 : numST)) * 100) + "%";

        gameStatus = GameStatus.Pause;
        break;

      case GameStatus.WinDance:
        level.gameObject.SetActive(false);
        SetAiming(Aiming.NotAiming);
        anim.Play("WinDance");
        audioGlobal.clip = WindDanceMusic;
        audioGlobal.loop = false;
        audioGlobal.Play();
        gameStatus = GameStatus.WinDance;
        break;

      case GameStatus.Death:
        gameStatus = GameStatus.Death;
        SetAiming(Aiming.NotAiming);
        if (crushed) {
          crushed = false;
          anim.Play("DeathCrush");
        }
        else if (Random.Range(0, 2) == 0) anim.Play("Death1");
        else anim.Play("Death2");

        // Reduce the numebr of lives.
        numLives--;
        if (numLives == 0) { // If -1 then show the game over
          SetGameStatus(GameStatus.GameOver);
          return;
        }
        // If not, pause for a while, update UI, and restart the level
        for (int i = 0; i < Lives.Length; i++) {
          Lives[i].enabled = i < numLives;
        }
        StartCoroutine(RestartLevel());
        break;

      case GameStatus.GameOver:
        gameStatus = GameStatus.GameOver;
        SetAiming(Aiming.NotAiming);
        GameOver.SetActive(true);
        break;
    }
  }

  private IEnumerator StartGame() {
    SetAiming(Aiming.NotAiming);
    yield return new WaitForSeconds(.2f);
    Ground = GameObject.FindObjectOfType<Terrain>(); // FIXME, remove it when the scenes will be merged

    for (int i = 0; i < Lives.Length; i++) {
      Lives[i].enabled = i < numLives;
    }
    currentLevel = -1;
    GoNextlevel();
  }

  private void Update() {
    if (Input.GetKeyDown(KeyCode.F11)) {
      bool full = !FullScreenToggle.isOn;
      FullScreenToggle.SetIsOnWithoutNotify(full);
      Screen.fullScreen = full;
      PlayerPrefs.SetInt("FullScreen", full ? 1 : 0);
    }

    switch (gameStatus) {
      case GameStatus.Intro: break;

      case GameStatus.Play:
        PlayUpdate();
        break;

      case GameStatus.Pause:
        if (Input.GetKeyDown(KeyCode.Escape)) SetGameStatus(GameStatus.Play);
        break;

      case GameStatus.WinDance:
        if ((audioGlobal.clip == WindDanceMusic && !audioGlobal.isPlaying) || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonUp(0)) {
          anim.SetBool("Aim", false);
          anim.Play("Idle");
          audioGlobal.clip = Crickets;
          audioGlobal.loop = true;
          audioGlobal.Play();
          if (GoNextlevel()) return; // No more levels if the return is true
          SetGameStatus(GameStatus.Play);
        }
        break;

      case GameStatus.Death: break;

      case GameStatus.GameOver:
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonUp(0)) {
          if (level != null) {
            level.RemoveAllEnemies();
            level.gameObject.SetActive(false);
          }

          SetGameStatus(GameStatus.Intro);
        }
        break;
    }
  }

  void PlayUpdate() {
    if (Input.GetKeyDown(KeyCode.Escape)) {
      SetGameStatus(GameStatus.Pause);
      return;
    }

    if (Input.GetMouseButtonDown(1)) { // Change aiming/no-aiming if we press the right mouse buton
      if (aiming == Aiming.NotAiming) SetAiming(Aiming.Loading);
      else SetAiming(Aiming.NotAiming);
    }

    float x = Input.GetAxis("Horizontal");
    if (x != 0) { // If we move we cannot be aiming
      SetAiming(Aiming.NotAiming);
    }

    if (aiming == Aiming.ArrowReady && Input.GetMouseButtonDown(0)) { // We should eb sure the aiming anim is completed
      audioBow.clip = ThrowArrow;
      audioBow.Play();
      arrowStart = HandRefR.position;
      arrowDir = (HandRefL.position - HandRefR.position + Vector3.up * .01f).normalized;
      anim.Play("Shoot");
      return;
    }


    if (x == 0 || aiming == Aiming.ArrowReady) { // not moving
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

    if (aiming != Aiming.NotAiming) {
      float aimH = 2f * (Input.mousePosition.x - Screen.width * .5f) / Screen.width - .09f;
      aimV = 2f * (Input.mousePosition.y - Screen.height * .5f) / Screen.height;
      anim.SetFloat("AimH", aimH);
      anim.SetFloat("AimV", aimV);

      // When aiming, the local rotation of the player should be looking to the center, and move pretty quick
      playerTargetAngle = Mathf.LerpAngle(playerTargetAngle, aimH * 85, 8 * Time.deltaTime);
      float playerCurrentAngle = player.localEulerAngles.y;
      float dist = Mathf.Abs(playerTargetAngle - playerCurrentAngle);
      player.localRotation = Quaternion.Euler(0, Mathf.Lerp(playerCurrentAngle, playerTargetAngle, dist * 15 * Time.deltaTime), 0);

      if (aiming == Aiming.ArrowReady) {
        Vector3 pos = HandRefR.position;
        Vector3 vel = (HandRefL.position - HandRefR.position + Vector3.up * .00995f).normalized * arrowforce;
        Vector3 acc = Physics.gravity;
        Vector3 cursor = Vector3.zero;
        float dt = Time.fixedDeltaTime / Physics.defaultSolverVelocityIterations;
        int count = 0;
        for (int i = 0; i < aimLine.Length; i++) {
          count++;
          aimLine[i] = pos;
          float t = 0;
          while (t < (aimV > 0 ? 2 : .8f) * Time.fixedDeltaTime) {
            vel += acc * dt;
            pos += vel * dt;
            t += dt;
          }
          if (pos.y < Ground.SampleHeight(pos) + .1f) {
            for (int j = i + 1; j < aimLine.Length; j++) {
              aimLine[j] = pos;
            }
            break;
          }
          if (cursor == Vector3.zero && (Vector3.Distance(pos, HandRefR.position) > 2 || pos.y < Ground.SampleHeight(pos) + .2f)) cursor = pos;
        }
        if (PlayerData.Difficulty == 0) {
          lineRenderer.positionCount = count;
          lineRenderer.SetPositions(aimLine);
        }
        if (PlayerData.Difficulty < 2)
          cursorPointer.cursorPosition = cursor;
        else
          cursorPointer.cursorPosition = Vector3.up * 20;
      }
    }
  }

  readonly Vector3[] aimLine = new Vector3[48];

  public void PlayerDeath(bool crush = false) {
    Debug.Log(">>>>>>>>>>>>>>>>>>>>>>>>>>>> death");
    crushed = crush;
    SetGameStatus(GameStatus.Death);
  }

  IEnumerator RestartLevel() {
    yield return new WaitForSeconds(4);
    SetGameStatus(GameStatus.Play);
  }

  public void WinLevel() {
    SetGameStatus(GameStatus.WinDance);
  }

  public Transform HandRefR, HandRefL;
  public float aimV;

  private void OnApplicationFocus(bool focus) {
    Cursor.visible = !focus || gameStatus == GameStatus.Pause || gameStatus == GameStatus.Intro || gameStatus == GameStatus.GameOver;
  }

  public float arrowforce = 100;

  public void ArrowLoaded() {
    SetAiming(Aiming.ArrowReady);
  }

  Vector3 arrowStart;
  Vector3 arrowDir;

  public void ArrowShoot() {
    if (aiming != Aiming.ArrowReady) return;
    PlayerData.AddAShoot();
    SetAiming(Aiming.NotAiming);
    audioBow.clip = BowDraw;
    audioBow.Play();
    if (Instantiate(ArrowPrefab).GetChild(0).TryGetComponent(out Arrow arrow)) {
      Quaternion rot = Quaternion.LookRotation(arrowDir, Vector3.up);
      arrow.Init(arrowStart, rot, arrowforce * arrowDir, Ground);
    }
  }

  internal void EnemyKilled(int done, int toWin) {
    PlayerData.AddAKill();
    LevelProgress.text = $"{level.GetName()}\n{done}/{level.GetToWin()}";
  }

  public void ClosePauseManu() {
    PauseMenu.SetActive(false);
    Time.timeScale = 1;
    SetGameStatus(GameStatus.Play);
  }

  [SerializeField] private Slider volumeMusic;
  [SerializeField] private Slider volumeSound;
  public void AlterVolume(bool music) {
    if (music) {
      MasterMixer.SetFloat("VolumeMusic", volumeMusic.value * 70 - 60);
      PlayerPrefs.SetFloat("VolumeMusic", volumeMusic.value);
    }
    else {
      MasterMixer.SetFloat("VolumeSounds", volumeSound.value * 70 - 60);
      PlayerPrefs.SetFloat("VolumeSounds", volumeSound.value);
    }
  }
  public Toggle FullScreenToggle;
  public void ToggleFullScreen() {
    Screen.fullScreen = FullScreenToggle.isOn;
    PlayerPrefs.SetInt("FullScreen", FullScreenToggle.isOn ? 1 : 0);
  }

  public TMP_Dropdown DifficultyDD;
  public void ChangeDifficulty() {
    PlayerData.Difficulty = DifficultyDD.value;
  }

  void SetAiming(Aiming a) {
    aiming = a;
    lineRenderer.enabled = (PlayerData.Difficulty == 0 && aiming == Aiming.ArrowReady);
    cursorPointer.aimingCursor = (PlayerData.Difficulty < 2 && aiming == Aiming.ArrowReady);
    anim.SetBool("Aim", a == Aiming.Loading);
    ArrowPlayer.SetActive(a == Aiming.ArrowReady);
    if (a == Aiming.Loading) {
      audioBow.clip = BowDraw;
      audioBow.Play();
    }
  }
}