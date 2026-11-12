using Godot;
using System;
using System.IO;
using System.Threading.Tasks;

[GlobalClass]
public partial class MoveComponent : Node
{
    //родительская нода
    protected CharacterBody3D ParentNode;
    //цель движения
    protected Node3D Target;
    //скорость движения
    [Export]
    protected float Speed;
    //Скорость движения отдельной переменной для изменения
    private float OriginalSpeed;
    //радиус остановки движения вблизи
    [Export]
    protected float StopRadius = 0.5f;

    //Таймер для модифицированной скорости
    private Timer ModifyTimer = new Timer();

    
    //флаг для возможности управляемости
    [Export]
    private bool IsControllable = false;

    //позиция, куда движется объект в случае отстутствия цели
    private Vector3 FallbackPosiiton = new Vector3(0, 0, 0);

    //задаем родительскую ноду
    public override void _Ready()
    {
        if (GetParent() is CharacterBody3D p)
        {
            ParentNode = p;
        }
        OriginalSpeed = Speed;
        ModifyTimer.Timeout += () => ReturnSpeed();
        ModifyTimer.OneShot = true;
        AddChild(ModifyTimer);
    }

    //Возвращает объекту скорость
    private void ReturnSpeed()
    {
        Speed = OriginalSpeed;
    }

    //сеттер запасной позиции
    public void SetFallbackPosition(Vector3 p)
    {
        FallbackPosiiton = p;
    }

    //задаем цель (сеттер таргета)
    public void SetTarget(Node3D target)
    {
        Target = target;
    }

    //каждый кадр движемся к цели если родительская нода существует и объект не управляемый
    //если объект управляемый то просто двигаем его в зависимости от ввода игрока
    public override void _PhysicsProcess(double delta)
    {
        if (ParentNode != null && !IsControllable)
        {
            Move(ParentNode, Target);
        } else if (ParentNode != null && IsControllable)
        {
        Vector2 inputVector = Input.GetVector("left", "right", "up", "down");
        Vector3 direction = new Vector3(inputVector.X, 0, inputVector.Y);
        
        ParentNode.Velocity = direction * Speed;
        ParentNode.MoveAndSlide();
        }
    }

    //двигаем родителя к цели, если ее нет - к центру экрана
    private void Move(CharacterBody3D p, Node3D t)
    {   
        Vector3 diff;
        if (t != null)
            diff = t.GlobalPosition - p.GlobalPosition;
        else
            diff = FallbackPosiiton - p.GlobalPosition;
        Vector3 direction = diff.Normalized();
        if (diff.Length() > StopRadius)
        {
            Vector3 velocity = new Vector3(
                direction.X * Speed,
                0,
                direction.Z * Speed
            );
            p.Velocity = velocity;
            p.MoveAndSlide();
        }
    }

    //функция, позволяющая на время изменить скорость компонента
    public void MultiplySpeed(float multiplier, float time)
    {
        Speed = OriginalSpeed * multiplier;
        ModifyTimer.Start(time);
    }
}
