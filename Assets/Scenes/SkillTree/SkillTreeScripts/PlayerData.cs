public static class PlayerData
{
    public static int Armor
    {
        get => _armor;
        set
        {
            _armor += value;
            _armor = _armor > 3 ? 3 : _armor;
        }
    }

    private static int _armor;

    public static int MoveSpeed
    {
        get => _moveSpeed;
        set
        {
            _moveSpeed += value;
            _moveSpeed = _moveSpeed > 3 ? 3 : _moveSpeed;
        }
    }

    private static int _moveSpeed;

    public static int AttackSpeed
    {
        get => _attackSpeed;
        set
        {
            _attackSpeed += value;
            _attackSpeed = _attackSpeed > 3 ? 3 : _attackSpeed;
        }
    }

    private static int _attackSpeed;

    public static int Accuracy
    {
        get => _accuracy;
        set
        {
            _accuracy += value;
            _accuracy = _accuracy > 3 ? 3 : _accuracy;
        }
    }

    private static int _accuracy;
    public static int Experience { get; private set; }

    public static void GainExperience(int experience)
    {
        Experience += experience;
    }

    public static void SpendExperience(int experience)
    {
        if (experience > Experience) return;
        Experience -= experience;
    }
}