using Godot;
using System;

[GlobalClass]
public partial class HealthComponent : Node
{   
    //родительская нода
    private Node3D parentNode;
    //максимальное здоровье
    [Export]
    protected float MaxHealth = 100f;
    //текцщее здоровье
    protected float CurrentHealth = 100f;

    //сигналы для получения урона и смерти объекта
    [Signal]
    public delegate void LostHealthEventHandler();
    [Signal]
    public delegate void DiedEventHandler();

    //здааем родителя и приравниваем здороьве к максимальному
    public override void _Ready()
    {
        parentNode = GetParent<Node3D>();
        CurrentHealth = MaxHealth;
    }

    //геттеры максимального и текцщего здоровья
    public float GetMaxHealth() { return MaxHealth; }
    public float GetCurrentHealth() { return CurrentHealth; }

    //получаем урон, при нуле объект умираем
    public void TakeDamage(float damage)
    {   
        CurrentHealth -= damage;
        if (CurrentHealth <= 0)
        {
            Die();
        }
        EmitSignal(SignalName.LostHealth);
    }

    //удаление объекта при смерти
    private void Die()
    {   
        EmitSignal(SignalName.Died);
        parentNode.QueueFree();
    }
}
