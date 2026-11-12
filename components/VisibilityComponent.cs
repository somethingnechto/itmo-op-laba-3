using Godot;
using System;

[GlobalClass]
public partial class VisibilityComponent : Area3D
{
    //родительская нода
    private Node3D ParentNode;
    //цель
    private Node3D Target;
    //ссылка на компонент стрельбы
    [Export]
    public ShootComponent shootComponent;
    //ссылка на компонент движения
    [Export]
    public MoveComponent moveComponent;
    //массив для всех найденных тел - потенциальных целей
    private Godot.Collections.Array<Node3D> detectedBodies = new Godot.Collections.Array<Node3D>();

    //задаем родительский объект и привязываем сигналы, сразу проверяем, не появился ли объект сразу в другом
    public override void _Ready()
    {
        ParentNode = GetParent<Node3D>();
        if (GetNodeOrNull<CollisionShape3D>("CollisionShape3D") != null)
        {
            BodyEntered += OnBodyEntered;
            BodyExited += OnBodyExited;   
            Godot.Collections.Array<Node3D> overlappingBodies = GetOverlappingBodies();
            for (int i = 0; i < overlappingBodies.Count; i++)
            {
                OnBodyEntered(overlappingBodies[i]);
            }
        }
    }

    //проверяем на совпадение групп при столкновении и добавляем в потенцаильные цели если группы не совпадают
    private void OnBodyEntered(Node3D body)
    {
        bool IsInSameGroup = false;
        if (body.IsInGroup("Enemy") && ParentNode.IsInGroup("Enemy")
        || body.IsInGroup("Tower") && ParentNode.IsInGroup("Tower"))
        {
            IsInSameGroup = true;
        }
        if (!IsInSameGroup)
        {
            HealthComponent h = body.GetNodeOrNull<HealthComponent>("HealthComponent");
            if (h != null)
            {
                detectedBodies.Add(body);
            }        
        }
    }

    //убираем потенциальную цель
    private void OnBodyExited(Node3D body)
    {
        detectedBodies.Remove(body);
    }

    //определяем ближайший объект из потенциальных и ставим его целью
    public override void _Process(double delta)
    {
        Node3D ClosestBody = null;
        for (int i = 0; i < detectedBodies.Count; i++)
        {
            Vector3 DistanceToTarget = GlobalPosition - detectedBodies[i].GlobalPosition;
            if ((ClosestBody == null) || (DistanceToTarget.Length() <= (GlobalPosition - ClosestBody.GlobalPosition).Length()))
            {
                ClosestBody = detectedBodies[i];
            }
        }
        Target = ClosestBody;
        if (shootComponent != null)
        {
            shootComponent.SetTarget(Target);
        }
        if (moveComponent != null)
        {
            moveComponent.SetTarget(Target);
        }
    }

}
