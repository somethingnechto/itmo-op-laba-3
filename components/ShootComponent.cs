using Godot;
using System;

[GlobalClass]
public partial class ShootComponent : Node3D
{
    //сцена пули
    [Export]
    private PackedScene BulletScene;
    //урон пули
    [Export]
    private float BulletDamage = 10f;
    //скорость стрельбы в пулях в секунду
    [Export]
    private float FireRate = 2f;
    //скорость полета пули
    [Export]
    private float BulletSpeed = 50f;

    //флаг управляемости (слежение за мышью и стрельба по нажатию кнопки)
    [Export]
    private bool IsControllable = false;

    //флаг возможности стрелять
    private bool IsAbleToShoot = true;

    //цель стерльбы
    private Node3D Target;
    //таймер стрельбы
    private Timer ShootTimer = new Timer();

    //настраиваем таймер и добавляем его в дерево
    public override void _Ready()
    {
        ShootTimer.WaitTime = 1.0f / FireRate;
        if (IsControllable) ShootTimer.OneShot = true;
        else {
            ShootTimer.OneShot = false;
            ShootTimer.Timeout += () => TryShoot();
        }
        AddChild(ShootTimer);
    }

    //пробуем выстрелить если сцена пули указана и можно стрелять
    private void TryShoot()
    {
        if (BulletScene != null && IsAbleToShoot)
            SpawnBullet();
    }

    //сеттер возможности стрелять
    public void SetAbilityToShoot(bool a)
    {
        IsAbleToShoot = a;
    }

    //создаем пулю
    private void SpawnBullet()
    {
        Bullet bullet = BulletScene.Instantiate<Bullet>();
        GetTree().CurrentScene.AddChild(bullet);
        bullet.GlobalPosition = GlobalPosition;
        bullet.GlobalRotation = GlobalRotation;
        bullet.SetSpeed(BulletSpeed);
        bullet.SetDamage(BulletDamage);
    }

    //включение стрельбы
    private void TurnOnCannon()
    {
        if (ShootTimer.IsStopped())
            ShootTimer.Start();
    }

    //выключение стрельбы
    private void TurnOffCannon()
    {
        if (!ShootTimer.IsStopped())
            ShootTimer.Stop();
    }

    //задаем цель (сеттер таргета)
    public void SetTarget(Node3D target)
    {
        Target = target;
    }

    //включаем стрельбу при наличии цели и выключаем при отсутствии цели
    //если пушкой можно управлять то вместо этого вращаем ее в направлении камеры и стреляем по нажатию кнопки
    public override void _Process(double delta)
    {
        if (!IsControllable) {
            if (Target != null && IsInstanceValid(Target))
            {
                Vector3 pos = new Vector3(Target.GlobalPosition.X, GlobalPosition.Y, Target.GlobalPosition.Z);
                LookAt(pos);
                TurnOnCannon();
            }
            else
            {
                Rotation += new Vector3(0, (float)delta * 0.5f, 0);
                TurnOffCannon();
            }
        } else
        {
            Camera3D camera = GetViewport().GetCamera3D();
            if (camera == null) return;
            Vector3 mouseWorldPos = camera.ProjectRayOrigin(GetViewport().GetMousePosition());
            Vector3 targetPoint = new Vector3(mouseWorldPos.X, GlobalPosition.Y, mouseWorldPos.Z);
            LookAt(targetPoint, Vector3.Up);

            if (Input.IsActionJustPressed("shoot") && ShootTimer.IsStopped())
            {
                TryShoot();
                ShootTimer.Start();
            }
        }
    }

}
