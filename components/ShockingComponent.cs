using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;

[GlobalClass]
public partial class ShockingComponent : DamagingComponent
{
    //дальность одного скачка
    [Export]
    private float JumpRange = 10.0f;

    //кол-во скачков
    [Export]
    private int Jumps = 2;


    //находим ближайших врагов и наносим им урон, одновременно рисуем луч для красивого визуала
    protected override void ActivateDamageEffect(CharacterBody3D body)
    {
        HealthComponent h = body.GetNodeOrNull<HealthComponent>("HealthComponent");
        if (h != null)
        {
            h.TakeDamage(Damage);
        }
        List<Vector3> points = new List<Vector3>{ new Vector3(body.GlobalPosition.X, GlobalPosition.Y, body.GlobalPosition.Z) };
        HashSet<Node3D> hitTargets = new HashSet<Node3D> { body };
        Vector3 currentJumpOrigin = body.GlobalPosition;

        var spaceState = GetWorld3D().DirectSpaceState;
        var query = new PhysicsShapeQueryParameters3D();
        var sphereShape = new SphereShape3D { Radius = JumpRange };
        query.Shape = sphereShape;
        query.CollisionMask = CollisionMask;
        for (int i = Jumps; i > 0; i--)
        {
            query.Transform = new Transform3D(Basis.Identity, currentJumpOrigin);
            var results = spaceState.IntersectShape(query);

            CharacterBody3D nextTarget = null;
            float closestDistSq = 999999.0f;

            foreach (var result in results)
            {
                if (result["collider"].Obj is CharacterBody3D candidate && !hitTargets.Contains(candidate))
                {
                    float distSq = currentJumpOrigin.DistanceSquaredTo(candidate.GlobalPosition);
                    if (distSq < closestDistSq)
                    {
                        closestDistSq = distSq;
                        nextTarget = candidate;
                    }
                }
            }
            if (nextTarget == null) break;

            hitTargets.Add(nextTarget);
            currentJumpOrigin = nextTarget.GlobalPosition;

            HealthComponent h2 = nextTarget.GetNodeOrNull<HealthComponent>("HealthComponent");
            if (h2 != null)
            {
                h2.TakeDamage(Damage);
                points.Add(new Vector3(nextTarget.GlobalPosition.X, GlobalPosition.Y, nextTarget.GlobalPosition.Z));
            }
        }
        MeshInstance3D meshInstance = CreateLightningLine(points);
        GetTree().CurrentScene.AddChild(meshInstance);
        GetTree().CreateTimer(0.1f).Timeout += () => meshInstance.QueueFree();
    }

    //вспомогательный метод, который создает линии для визуального отображения молнии
    private MeshInstance3D CreateLightningLine(List<Vector3> points)
    {
        var meshInstance = new MeshInstance3D();
        var immediateMesh = new ImmediateMesh();

        if (points == null || points.Count < 2)
        {
           meshInstance.Mesh = immediateMesh;
            return meshInstance;
        }

        var material = new StandardMaterial3D
        {
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            VertexColorUseAsAlbedo = true
        };

        immediateMesh.SurfaceBegin(Mesh.PrimitiveType.LineStrip, material);
        foreach (Vector3 point in points)
        {
            immediateMesh.SurfaceSetColor(new Godot.Color(0.8f, 0.8f, 1.0f));
            immediateMesh.SurfaceAddVertex(point);
        }
        immediateMesh.SurfaceEnd();

        meshInstance.Mesh = immediateMesh;
        return meshInstance;
    }
}
