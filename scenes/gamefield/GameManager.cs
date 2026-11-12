using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

public partial class GameManager : Node3D
{
    //текущий режим игрока
    private string PlayerMode = "fight";

    //контейнер кнопок для спавна башен
    [Export]
    private HBoxContainer HBoxContainer;

    //"призрак" башни для режима строительства
    [Export]
    private Area3D TowerGhost;
    //флаг для возможности установить башню
    private bool IsTowerPlaceable;

    //нода ядра и его healthComponent
    [Export]
    private CharacterBody3D Core;
    private HealthComponent CoreHealthComponent;

    //текст, отображающий хп ядра на экране
    [Export]
    private Label CoreHealthLabel;

    //текст, отображающий хп игрока на экране
    [Export]
    private Label PlayerHealthLabel;

    //количество валюты игрока
    private int Money = 0;
    //текст, отображающий количество валюты/металлолома игрока
    [Export]
    private Label MoneyAmountLabel;

    //Массивы врагов и башен на карте, а также объект игрока и сцена игрока
    List<CharacterBody3D> Enemies = new List<CharacterBody3D>();
    List<CharacterBody3D> Towers = new List<CharacterBody3D>();
    [Export]
    PackedScene PlayerScene;
    [Export]
    CharacterBody3D Player;

    //массивы возможных врагов и башен для спавна
    [Export]
    Godot.Collections.Array<PackedScene> EnemyScenes;
    [Export]
    Godot.Collections.Array<PackedScene> TowerScenes;

    //время респавна игрока
    [Export]
    private float RespawnTime = 3.0f;

    //время между волнами и номер текущей волны
    [Export]
    private float TimeBetweenWaves = 10.0f;
    private int WaveNumber = 0;
    
    //Таймер, отвечающий за спавн волн
    private Timer WaveTimer = new Timer();

    private RandomNumberGenerator random = new RandomNumberGenerator();

    //находим healthcomponent у ядра и подключаемся к сигналам, создаем таймер волн
    public override void _Ready()
    {
        CoreHealthComponent = Core.GetNodeOrNull<HealthComponent>("HealthComponent");
        if (CoreHealthComponent != null)
        {
            CoreHealthComponent.LostHealth += () => UpdateCoreHealthLabel();
            CoreHealthComponent.Died += () => QuitGame();
        }

        ConnectPlayer(Player);
        UpdateCoreHealthLabel();
        UpdateMoneyLabel();

        WaveTimer.WaitTime = TimeBetweenWaves;
        WaveTimer.OneShot = false;
        WaveTimer.Timeout += () => OnWaveTimerTimeout();
        AddChild(WaveTimer);
        WaveTimer.Start();
        OnWaveTimerTimeout();
    }

    //спавним врагов каждую волну
    private void OnWaveTimerTimeout()
    {
        WaveNumber++;
        for (int i = 0; i < Math.Floor(-1 + Math.Sqrt(8 * WaveNumber + 1)); i++)
        {
            int enemyId = random.RandiRange(0, 1);
            CharacterBody3D enemyInstance = EnemyScenes[enemyId].Instantiate<CharacterBody3D>();
            Enemies.Add(enemyInstance);
            AddChild(enemyInstance);
            enemyInstance.GlobalPosition = GetSpawnPosition();
            HealthComponent h = enemyInstance.GetNodeOrNull<HealthComponent>("HealthComponent");
            if (h != null)
            {
                h.Died += () => OnEnemyDeath(enemyInstance);
            }
        }
    }

    //меняем режим по нажатию кнопки
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("build_mode_change") && IsInstanceValid(Player) && Player != null)
        {
            if (PlayerMode == "fight") ChangePlayerMode("build");
            else if (PlayerMode == "build") ChangePlayerMode("fight");
        }
    }

    //меняем режим игрока и его поведение
    private void ChangePlayerMode(string mode)
    {
        if (mode == "build")
        {
            PlayerMode = "build";
            ShootComponent s = Player.GetNodeOrNull<ShootComponent>("ShootComponent");
            if (s != null)
            {
                s.SetAbilityToShoot(false);
            }
            HBoxContainer.Visible = true;
            TowerGhost.Visible = true;
        } else if (mode == "fight")
        {
            IsTowerPlaceable = false;
            PlayerMode = "fight";
            ShootComponent s = Player.GetNodeOrNull<ShootComponent>("ShootComponent");
            if (s != null)
            {
                s.SetAbilityToShoot(true);
            }
            HBoxContainer.Visible = false;
            TowerGhost.Visible = false;
        }
    }

    //меняем положение и цвет "призрачной" башни (если она видимая)
    public override void _PhysicsProcess(double delta)
    {
        if (TowerGhost.Visible == true && IsInstanceValid(Player) && Player != null)
        {
            TowerGhost.GlobalPosition = Player.GlobalPosition + new Vector3(0f, 0f, 2.0f);
            if (TowerGhost.GetOverlappingBodies().Count > 0)
            {   
                IsTowerPlaceable = false;
                MeshInstance3D meshInstance3D = TowerGhost.GetNodeOrNull<MeshInstance3D>("MeshInstance3D");
                MeshInstance3D meshInstance3D2 = TowerGhost.GetNodeOrNull<MeshInstance3D>("MeshInstance3D2");
                if (meshInstance3D.GetSurfaceOverrideMaterial(0) is StandardMaterial3D mat) mat.AlbedoColor = new Color(1.0f, 0.0f, 0.0f, 0.5f);
                if (meshInstance3D2.GetSurfaceOverrideMaterial(0) is StandardMaterial3D mat2) mat2.AlbedoColor = new Color(1.0f, 0.0f, 0.0f, 0.5f);
            } else if (TowerGhost.GetOverlappingBodies().Count == 0)
            {
                IsTowerPlaceable = true;
                MeshInstance3D meshInstance3D = TowerGhost.GetNodeOrNull<MeshInstance3D>("MeshInstance3D");
                MeshInstance3D meshInstance3D2 = TowerGhost.GetNodeOrNull<MeshInstance3D>("MeshInstance3D2");
                if (meshInstance3D.GetSurfaceOverrideMaterial(0) is StandardMaterial3D mat) mat.AlbedoColor = new Color(0.0f, 1.0f, 0.0f, 0.5f);
                if (meshInstance3D2.GetSurfaceOverrideMaterial(0) is StandardMaterial3D mat2) mat2.AlbedoColor = new Color(0.0f, 1.0f, 0.0f, 0.5f);
            }
        }   
    }



    //выдаем валюту игроку и удаляем противника в случае его смерти
    private void OnEnemyDeath(CharacterBody3D enemy)
    {
        Money += 10;
        UpdateMoneyLabel();
        Enemies.Remove(enemy);
    }

    //возвращает позицию спавна игрока
    private Vector3 GetSpawnPosition()
    {
        float width = 46f;
        float depth = 24f;
        float halfW = width / 2f;
        float halfD = depth / 2f;

        float perimeter = 2f * (width + depth);
        float dist = random.RandfRange(0f, perimeter);

        if (dist < width)
        {
            return new Vector3(-halfW + dist, 0f, -halfD);
        }
        dist -= width;

        if (dist < width)
        {

            return new Vector3(-halfW + dist, 0f, halfD);
        }
        dist -= width;

        if (dist < depth)
        {
            return new Vector3(-halfW, 0f, -halfD + dist);
        }
        dist -= depth;

        return new Vector3(halfW, 0f, -halfD + dist);
    }

    //подключаем ноду к переменной игрока и подключаем сигнал смерти
    private void ConnectPlayer(CharacterBody3D playerNode)
    {
        Player = playerNode;
        var playerHealth = Player.GetNodeOrNull<HealthComponent>("HealthComponent");
		if (playerHealth != null)
		{
			playerHealth.Died += OnPlayerDied;
            playerHealth.LostHealth += UpdatePlayerHealthLabel;
		}
        UpdatePlayerHealthLabel();
    }

    //респавним игрока через время
    private async void OnPlayerDied()
    {   
        ChangePlayerMode("fight");
        await ToSignal(GetTree().CreateTimer(RespawnTime), SceneTreeTimer.SignalName.Timeout);
        RespawnPlayer();
    }
    private void RespawnPlayer()
    {
        CharacterBody3D playerInstance = PlayerScene.Instantiate<CharacterBody3D>();
        playerInstance.GlobalPosition = new Vector3(0f, 0f, 3.0f);
        AddChild(playerInstance);
        ConnectPlayer(playerInstance);
    }

    //обновляют текст, отображающий хп и деньги
    private void UpdateCoreHealthLabel()
    {
        if (CoreHealthComponent != null) {
            string cur_hp = CoreHealthComponent.GetCurrentHealth().ToString(); 
            string max_hp = CoreHealthComponent.GetMaxHealth().ToString();
            CoreHealthLabel.Text = "Core health " + cur_hp + "/" + max_hp;
        }
    }
    private void UpdatePlayerHealthLabel()
    {
        if (IsInstanceValid(Player) && Player != null)
        {
            HealthComponent h = Player.GetNodeOrNull<HealthComponent>("HealthComponent");
            if (h != null) {
                string cur_hp = h.GetCurrentHealth().ToString(); 
                string max_hp = h.GetMaxHealth().ToString();
                PlayerHealthLabel.Text = "Player health " + cur_hp + "/" + max_hp;
            }
        }
    }
    private void UpdateMoneyLabel()
    {
        MoneyAmountLabel.Text = "Scrap: " + Money.ToString();
    }

    //выход из игры
    private void QuitGame()
    {
        GetTree().Quit();
    }


    //функции для спавна башен по нажатию соответствующих кнопок
    private void OnTowerButtonPressed()
    {
        if (PlayerMode == "build" && Money >= 10 && IsTowerPlaceable)
        {
            Money -= 10;
            UpdateMoneyLabel();
            CharacterBody3D tower = TowerScenes[0].Instantiate<CharacterBody3D>();
            AddChild(tower);
            tower.GlobalPosition = TowerGhost.GlobalPosition;
        }
    }
    private void OnLightningButtonPressed()
    {
        if (PlayerMode == "build" && Money >= 20 && IsTowerPlaceable)
        {
            Money -= 20;
            UpdateMoneyLabel();
            CharacterBody3D tower = TowerScenes[1].Instantiate<CharacterBody3D>();
            AddChild(tower);
            tower.GlobalPosition = TowerGhost.GlobalPosition;
        }
    }
    private void OnFreezeButtonPressed()
    {
        if (PlayerMode == "build" && Money >= 20 && IsTowerPlaceable)
        {
            Money -= 20;
            UpdateMoneyLabel();
            CharacterBody3D tower = TowerScenes[2].Instantiate<CharacterBody3D>();
            AddChild(tower);
            tower.GlobalPosition = TowerGhost.GlobalPosition;
        }
    }
}
