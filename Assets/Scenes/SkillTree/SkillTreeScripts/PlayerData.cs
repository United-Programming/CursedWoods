using UnityEngine;

public static class PlayerData {
  /// <summary>
  /// Gets and sets the Armor level
  /// </summary>
  public static int Armor {
    get => _armor;
    set {
      _armor += value;
      _armor = _armor > 3 ? 3 : _armor;
    }
  }

  private static int _armor;

  /// <summary>
  /// Gets and sets the Movement speed
  /// </summary>
  public static int MoveSpeed {
    get => _moveSpeed;
    set {
      _moveSpeed += value;
      _moveSpeed = _moveSpeed > 3 ? 3 : _moveSpeed;
    }
  }

  private static int _moveSpeed;

  /// <summary>
  /// Gets and sets the bow reload speed
  /// </summary>
  public static int AttackSpeed {
    get => _attackSpeed;
    set {
      _attackSpeed += value;
      _attackSpeed = _attackSpeed > 3 ? 3 : _attackSpeed;
    }
  }

  private static int _attackSpeed;

  /// <summary>
  /// Gets and sets the precision of the arrows
  /// </summary>
  public static int Accuracy {
    get => _accuracy;
    set {
      _accuracy += value;
      _accuracy = _accuracy > 3 ? 3 : _accuracy;
    }
  }

  private static int _accuracy;
  public static int Experience { get; private set; }

  public static void GainExperience(int experience) {
    Experience += experience;
  }

  public static void SpendExperience(int experience) {
    if (experience > Experience) return;
    Experience -= experience;
  }

  /// <summary>
  /// Sets all experience values to zero to start a new game
  /// </summary>
  public static void ResetStats() {
    _armor = 0;
    _accuracy = 0;
    _attackSpeed = 0;
    _moveSpeed = 0;
    _gameShoots = 0;
    _gameKills = 0;
  }


  private static int _difficulty = 1;
  /// <summary>
  /// Gets and sets the Difficulty value
  /// </summary>
  public static int Difficulty {
    get => _difficulty;
    set {
      _difficulty = value;
    }
  }
  /// <summary>
  /// Returns the difficulty multiplies (.9, 1, 1.1) used to change the speed of monsters
  /// </summary>
  public static float DifficultyMultiplier {
    get => .9f + _difficulty * .1f;
  }



  public static void PlayAnotherGame() {
    PlayerPrefs.SetInt("NumberOfPlays", PlayerPrefs.GetInt("NumberOfPlays", 0) + 1);
  }

  public static void AddAShoot() {
    PlayerPrefs.SetInt("NumberOfShoots", PlayerPrefs.GetInt("NumberOfShoots", 0) + 1);
    _gameShoots++;
  }
  public static void AddAKill() {
    PlayerPrefs.SetInt("NumberOfKills", PlayerPrefs.GetInt("NumberOfKills", 0) + 1);
    _gameKills++;
  }



  private static int _gameShoots, _gameKills;
  /// <summary>
  /// Gets and sets the Difficulty value
  /// </summary>
  public static int GameNumberOfShoots {
    get => _gameShoots;
    set {
      _gameShoots = value;
    }
  }
  /// <summary>
  /// Gets and sets the Difficulty value
  /// </summary>
  public static int GameNumberOfKills {
    get => _gameKills;
    set {
      _gameKills = value;
    }
  }


}