using Godot;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

[GlobalClass]
public partial class DamagingComponent : Area3D
{
    //урон
    [Export]
    protected float Damage = 10f;

    //родительская нода
    protected Node3D parentNode;

    //флаг для уничтожения при контакте с врагом
    private bool DestroyOnContact = false;

    //время между атаками(в случае если родитель не уничтожается)
    [Export]
    private float TimeBetweenAttacks = 2.0f;

    //таймер атак
    private Timer AttackTimer = new Timer();

    //список всех объектов которым наноим урон при контакте
    private List<CharacterBody3D> TargetsInRange = new List<CharacterBody3D>();

    //находит родительскую ноду, задает параметры таймера
    public override void _Ready()
    {
        parentNode = GetParent<Node3D>();
        if (!DestroyOnContact) {
            AttackTimer.WaitTime = TimeBetweenAttacks;
            AttackTimer.OneShot = false;
            AddChild(AttackTimer);
            AttackTimer.Timeout += () => DealDamageInRange();
        }
    }

    //наносим урон всем объектам в списке целей
    private void DealDamageInRange()
    {
        foreach (CharacterBody3D target in TargetsInRange)
        {
            if (!IsInstanceValid(target)) TargetsInRange.Remove(target);
        }

        foreach (CharacterBody3D target in TargetsInRange)
        {
            ActivateDamageEffect(target);
        }

        if (TargetsInRange.Count == 0)
        {
            AttackTimer.Stop();
        }
    }

    //убираем объекты, которые перестают пересекать Area
    private void OnBodyExited(Node3D body)
    {
        if (body is CharacterBody3D character)
        {
            if (character != null && TargetsInRange.Contains(character))
            {
                TargetsInRange.Remove(character);
                if (TargetsInRange.Count == 0)
                {
                    AttackTimer.Stop();
                }
            }
        }
    }

    //сеттер для установки флажка уничтожения при контакте
    public void SetDestroyMode(bool mode)
    {
        DestroyOnContact = mode;
    }


    //сеттер урона
    public void SetDamage(float d)
    {
        Damage = d;
    }

    //наносит урон при столкновении и уничтожает родителя при контакте если стоит флажок
    public void OnAreaBodyEntered(Node3D body) 
    {
        if (body is CharacterBody3D character)
        {
            ActivateDamageEffect(character);
            if (DestroyOnContact) parentNode.QueueFree();
            else
            {
                if (AttackTimer.IsStopped()) AttackTimer.Start();
                if (!TargetsInRange.Contains(character)) TargetsInRange.Add(character);
            }
        }
    }

    //виртуальный метод нанесения урона, который будем перезаписывать
    protected virtual void ActivateDamageEffect(CharacterBody3D body)
    {
        HealthComponent h = body.GetNodeOrNull<HealthComponent>("HealthComponent");
        if (h != null)
        {
            h.TakeDamage(Damage);
        }
    }
}
