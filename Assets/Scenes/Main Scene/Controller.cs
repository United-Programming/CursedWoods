using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Controller : MonoBehaviour {

  [Header("Player ----------------------")]
  public Transform Player;
  public GameObject ArrowPlayer;
  public Transform ArrowPrefab;
  public Animator anim;
  public LineRenderer lineRenderer;
  public int numLives = 3;
  public Image[] Lives;
  public float speed = .18f;
  bool crushed = false;
  float playerTargetAngle = 0;
  float movement = 0;
  float angle = 0;

  [Header("World ----------------------")]
  public Camera cam;
  public GameObject Environmnet;
  public Terrain Ground1;
  public Terrain Ground2;
  public Fog fog;
  public Aiming aiming = Aiming.NotAiming;
  public Transform HandRefR, HandRefL;
  float aimV;
  public float arrowforce = 25;

  [Header("UI ----------------------")]
  public CursorPointer cursorPointer;
  [SerializeField] GameObject GamePlay, Intro, GameCanvas, PauseMenu;
  public Toggle FullScreenToggle;
  public TMP_Dropdown DifficultyDD;
  public GameObject Fade;
  public TextMeshProUGUI fadeLevelText1;
  public TextMeshProUGUI fadeLevelText2;
  public CanvasGroup fadeCanvasGroup;
  public CanvasGroup textCanvasGroup;

  [Header("Audio ----------------------")]
  public AudioMixer MasterMixer;
  public AudioSource audioBow;
  public AudioSource audioGlobal;
  public AudioClip WindDanceMusic;
  public AudioClip Crickets;
  public AudioClip BowDraw;
  public AudioClip ThrowArrow;
  [SerializeField] private Slider volumeMusic;
  [SerializeField] private Slider volumeSound;

  [Header("Gameplay ----------------------")]
  public GameStatus gameStatus = GameStatus.GameOver;
  public GameObject GameOver;
  public TextMeshProUGUI LevelProgress;
  public TextMeshProUGUI NumOfPlay;
  public TextMeshProUGUI NumOfKillsG;
  public TextMeshProUGUI NumOfKillsT;
  public TextMeshProUGUI NumOfShootsG;
  public TextMeshProUGUI NumOfShootsT;
  public TextMeshProUGUI NumOfAccuracyG;
  public TextMeshProUGUI NumOfAccuracyT;
  int currentLevel;
  Level level;
  public Level[] Levels;

  [Header("Debug ----------------------")]
  public TMP_Dropdown debugStartLevel;



  // no aiming, loading, aiming with arrow ready
  public enum Aiming { NotAiming, Loading, ArrowReady };

  public enum GameStatus {
    Intro, BeginPlay, Play, Pause, WinDance, Death, GameOver
  }


  static Controller _c;
  public TextMeshProUGUI DbgTxt;

  public static void Dbg(string txt) {
    if (_c == null) return;
    _c.DbgTxt.text = txt;
  }

  private IEnumerator Start() {
    _c = this;
    yield return new WaitForSeconds(.2f);
    SetGameStatus(GameStatus.Intro);
    Dbg("ready");
  }


  public void StartNewGame() {
    StartLevel(true);
  }
  private void StartLevel(bool begin) {
    gameStatus = GameStatus.BeginPlay;
    if (begin) {
      PlayerData.ResetStats();
      PlayerData.PlayAnotherGame();
      numLives = 3;
      currentLevel = -1;
      if (debugStartLevel.value != 0) currentLevel = debugStartLevel.value;
    }
    StartCoroutine(StartingLevel());
  }

  IEnumerator StartingLevel() {
    yield return null;

    // Find what level to load and load it
    if (currentLevel == -1) currentLevel = 0;
    else {
      if (debugStartLevel.value != 0) currentLevel = debugStartLevel.value;
      else {
        currentLevel++;
        if (currentLevel >= Levels.Length) {
          LevelProgress.text = $"No more available levels!";
          SetGameStatus(GameStatus.Intro);
          Debug.Log("--------------------------------------- No more available levels!");
          yield break;
        }
      }
    }

    // Fade to black
    fadeCanvasGroup.alpha = 0;
    textCanvasGroup.alpha = 0;
    Fade.SetActive(true);
    Level l = Levels[currentLevel];
    fadeLevelText1.text = l.GetName();
    fadeLevelText2.text = l.GetName();
    yield return null;

    float alpha = 0;
    while (alpha < 1) {
      alpha += Time.deltaTime * 4.5f;
      fadeCanvasGroup.alpha = alpha;
      yield return null;
    }
    fadeCanvasGroup.alpha = 1;
    yield return null;
    alpha = 0;
    while (alpha < 1) {
      alpha += Time.deltaTime * 3f;
      textCanvasGroup.alpha = alpha;
      yield return null;
    }
    textCanvasGroup.alpha = 1;

    // Disable intro, just in case
    Intro.SetActive(false);

    // reset level and aiming
    if (currentLevel == 0 || /* FIXME */ currentLevel == debugStartLevel.value) {
      SetAiming(Aiming.NotAiming);
      GameCanvas.SetActive(true);
      PauseMenu.SetActive(false);
      Player.gameObject.SetActive(true);
      Environmnet.SetActive(true);
      GameOver.SetActive(false);
      GamePlay.SetActive(true);
      SetPlayerHeightPosition();
    }

    // Set the level
    if (level != null) level.gameObject.SetActive(false);
    level = Levels[currentLevel];
    level.gameObject.SetActive(true);
    level.Init(GetTerrainForLevel(level), this, false);
    LevelProgress.text = $"{level.GetName()}\n0/{level.GetToWin()}";
    transform.position = level.GetLevelCenter();
    SetPlayerHeightPosition();

    // wait a few if necessary
    float delay = 0;
    while (delay < 3) {
      delay += Time.deltaTime;
      yield return null;
      if (Input.GetMouseButtonUp(0)) delay = 5;
    }

    // Fade to visible
    yield return null;
    alpha = 1;
    while (alpha > 0) {
      yield return null;
      alpha -= Time.deltaTime * 1.5f;
      fadeCanvasGroup.alpha = alpha;
      textCanvasGroup.alpha = alpha;
    }
    yield return null;
    fadeCanvasGroup.alpha = 0;
    textCanvasGroup.alpha = 0;
    fadeLevelText1.text = "";
    fadeLevelText2.text = "";
    Fade.SetActive(false);

    // Play
    SetGameStatus(GameStatus.Play);
  }

  IEnumerator RestartLevel() {
    yield return new WaitForSeconds(4);
    SetGameStatus(GameStatus.Play);
  }

  private void SetGameStatus(GameStatus status) {
    if (status == gameStatus) return;

    switch (status) {
      case GameStatus.Intro:
        Intro.SetActive(true);
        GameCanvas.SetActive(false);
        PauseMenu.SetActive(false);
        Fade.SetActive(false);
        Player.gameObject.SetActive(false);
        Environmnet.SetActive(false);
        GameOver.SetActive(false);
        GamePlay.SetActive(false);
        lineRenderer.enabled = false;
        Cursor.visible = true;
        gameStatus = GameStatus.Intro;
        break;

      case GameStatus.BeginPlay:
        Cursor.visible = false;
        lineRenderer.enabled = false;
        break;

      case GameStatus.Play:
        Intro.SetActive(false);
        GameCanvas.SetActive(true);
        PauseMenu.SetActive(false);
        Fade.SetActive(false);
        Player.gameObject.SetActive(true);
        Environmnet.SetActive(true);
        GameOver.SetActive(false);
        GamePlay.SetActive(true);
        lineRenderer.enabled = false;
        Cursor.visible = false;
        Time.timeScale = 1;
        if (gameStatus == GameStatus.Death) {
          anim.Play("Idle");
          level.Init(GetTerrainForLevel(level), this, true);
        }
        SetPlayerHeightPosition();
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

  private Terrain GetTerrainForLevel(Level level) {
    if (level.GetLevelCenter().z < 500) return Ground1;
    else return Ground2;
  }

  private void SetPlayerHeightPosition() {
    Vector3 pos = transform.position;
    Terrain g = Player.position.z < 500? Ground1 : Ground2;
    pos.y = g.SampleHeight(Player.position) + .01f; // Find if we need Ground1 or 2
    transform.position = pos;
  }

  private void Update() {
    if (Input.GetKeyDown(KeyCode.P)) {
      if (fog.fogEnabled) fog.Disable();
      else fog.EnableFog(.9f);
    }

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
          StartLevel(false);
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

    if (!Player.gameObject.activeSelf) return;

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
      float playerCurrentAngle = Player.localEulerAngles.y;
      float dist = Mathf.Abs(playerTargetAngle - playerCurrentAngle);
      Player.localRotation = Quaternion.Euler(0, Mathf.Lerp(playerCurrentAngle, playerTargetAngle, dist * 10 * Time.deltaTime), 0);
      SetPlayerHeightPosition();


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
      float playerCurrentAngle = Player.localEulerAngles.y;
      float dist = Mathf.Abs(playerTargetAngle - playerCurrentAngle);
      Player.localRotation = Quaternion.Euler(0, Mathf.Lerp(playerCurrentAngle, playerTargetAngle, dist * 15 * Time.deltaTime), 0);

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
          float groundHeight = pos.z < 500 ? Ground1.SampleHeight(pos) : Ground2.SampleHeight(pos);
          if (pos.y < groundHeight + .1f) {
            for (int j = i + 1; j < aimLine.Length; j++) {
              aimLine[j] = pos;
            }
            break;
          }
          if (cursor == Vector3.zero && (Vector3.Distance(pos, HandRefR.position) > 2 || pos.y < groundHeight + .2f)) cursor = pos;
        }
        if (PlayerData.Difficulty == 0) {
          lineRenderer.enabled = aiming == Aiming.ArrowReady;
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
    crushed = crush;
    SetGameStatus(GameStatus.Death);
  }


  public void WinLevel() {
    SetGameStatus(GameStatus.WinDance);
  }


  private void OnApplicationFocus(bool focus) {
    Cursor.visible = !focus || gameStatus == GameStatus.Pause || gameStatus == GameStatus.Intro || gameStatus == GameStatus.GameOver;
  }


  public void ArrowLoaded() {
    SetAiming(Aiming.ArrowReady);
  }

  Vector3 arrowStart;
  Vector3 arrowDir;

  public void ArrowShoot() {
    if (aiming != Aiming.ArrowReady) return;
    PlayerData.AddAShoot();
    SetAiming(Aiming.Loading);
    if (Instantiate(ArrowPrefab).GetChild(0).TryGetComponent(out Arrow arrow)) {
      Quaternion rot = Quaternion.LookRotation(arrowDir, Vector3.up);
      arrow.Init(arrowStart, rot, arrowforce * arrowDir, arrowStart.z < 500 ? Ground1 : Ground2, this);
    }
  }

  internal void ArrowHit(Vector3 position) {
    if (level != null) level.ArrowhitAlert(position);
  }


  internal void EnemyKilled(int done) {
    PlayerData.AddAKill();
    LevelProgress.text = $"{level.GetName()}\n{done}/{level.GetToWin()}";
  }

  public void ClosePauseManu() {
    PauseMenu.SetActive(false);
    Time.timeScale = 1;
    SetGameStatus(GameStatus.Play);
  }


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
  public void ToggleFullScreen() {
    Screen.fullScreen = FullScreenToggle.isOn;
    PlayerPrefs.SetInt("FullScreen", FullScreenToggle.isOn ? 1 : 0);
  }

  public void ChangeDifficulty() {
    PlayerData.Difficulty = DifficultyDD.value;
  }

  void SetAiming(Aiming a) {
    aiming = a;
    lineRenderer.enabled = false;
    cursorPointer.aimingCursor = (PlayerData.Difficulty < 2 && aiming == Aiming.ArrowReady);
    if (Player.gameObject.activeSelf) anim.SetBool("Aim", a != Aiming.NotAiming);
    ArrowPlayer.SetActive(a == Aiming.ArrowReady);
    if (a == Aiming.Loading) {
      audioBow.clip = BowDraw;
      audioBow.Play();
    }
  }



  public void ShakeCamera(float amount) {
    StartCoroutine(ShakeCameraCR(amount));
  }

  IEnumerator ShakeCameraCR(float amount) {
    Transform camPos = Camera.main.transform;
    Vector3 pos = camPos.localPosition;
    yield return null;

    float time = 0;
    while (time < 1.5f) {
      time += Time.deltaTime;

      float step = Mathf.Sin(time * Mathf.PI * .6f) * amount;

      camPos.localPosition = pos + step * Random.Range(-1f, 1f) * Vector3.up + step * Random.Range(-1f, 1f) * Vector3.right + step * Random.Range(-1f, 1f) * Vector3.forward;
      yield return null;
    }
    camPos.localPosition = pos;
  }
} 