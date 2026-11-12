using Godot;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

[GlobalClass]
public partial class FreezingComponent : DamagingComponent
{   

    //Модификатор заммедления цели
    [Export]
    private float SlowdownModifier = 0.5f;

    //Время замедления цели
    [Export]
    private float SlowdownTime = 0.5f;

    //замедляем врага на время при нанесении урона
    protected override void ActivateDamageEffect(CharacterBody3D body)
    {
        HealthComponent h = body.GetNodeOrNull<HealthComponent>("HealthComponent");
        if (h != null)
        {
            h.TakeDamage(Damage);
        }
        MoveComponent m = body.GetNodeOrNull<MoveComponent>("MoveComponent");
        if (m != null) {
            m.MultiplySpeed(SlowdownModifier, SlowdownTime);
        }
    }
}