using Godot;
using System;

//класс пули
public partial class Bullet : Area3D
{
    //скорость пули
    private float Speed = 100f;

    //время до уничтожения (магическое число просто чтоб было)
    private float DestructionTime = 10.0f;
    //таймер, уничтожающий пулю через время
    private Timer DestructionTimer = new Timer();

    //ссылка на компонент для нанесения урона
    [Export]
    private DamagingComponent damagingComponent;

    //задаем уничтожене пули при контакте с врагом и создаем таймер, который удалит пулю по истечению времени
    public override void _Ready()
    {
        damagingComponent.SetDestroyMode(true);
        DestructionTimer.OneShot = true;
        DestructionTimer.WaitTime = 10.0f;
        AddChild(DestructionTimer);
        DestructionTimer.Timeout += () => QueueFree();
        DestructionTimer.Start();
    }


    //сеттер для скорости
    public void SetSpeed(float s)
    {
        Speed = s;
    }

    //сеттер для урона
    public void SetDamage(float d)
    {
        damagingComponent.SetDamage(d);
    }


    //двигает пулю каждый кадр
    public override void _PhysicsProcess(double delta)
    {
        Position += -Transform.Basis.Z * Speed * (float)delta;
    }
}
