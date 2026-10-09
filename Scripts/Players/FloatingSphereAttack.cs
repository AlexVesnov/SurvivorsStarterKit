using Godot;
using System.Collections.Generic;
using System.Linq;
using SuperheroSurvivors.Core;

/// Авто-оружие: сферы, вращающиеся вокруг героя. Сферы активны постоянно
/// (в ките периодически пропадали — упрощено: постоянный урон по площади).
public partial class FloatingSphereAttack : AutoWeapon
{
    [Export]
    public uint InitialSpheres = 1;

    [Export]
    public float Damages = 3f;

    [Export]
    public float RotationSpeed { get; private set; } = 2.5f;

    [Export]
    public float SphereDistance { get; private set; } = 1.4f;

    [Export]
    private PackedScene _spherePrefab;

    private readonly List<Area3D> _spheres = new();

    public override string WeaponId => "floating_sphere";

    public override void _Ready()
    {
        foreach (int _ in Enumerable.Range(0, (int)InitialSpheres))
            AddSphere();
    }

    public void AddSphere()
    {
        Area3D area = _spherePrefab.Instantiate<Area3D>();
        area.BodyEntered += OnBodyEntered;
        AddChild(area);
        _spheres.Add(area);
        RepositionSpheres();
    }

    public override void _PhysicsProcess(double delta)
    {
        RotateY((float)delta * RotationSpeed);
    }

    private void OnBodyEntered(Node body)
    {
        if (body is Enemy enemy) DealDamage(enemy, Damages, DamageType.Kinetic);
    }

    private void RepositionSpheres()
    {
        Vector3 basePosition = new(SphereDistance, 0.3f, 0);
        for (int i = 0; i < _spheres.Count; i++)
        {
            float axis = Mathf.Lerp(0, 360, (float)i / _spheres.Count);
            Basis basis = new(new Vector3(0, 1, 0), Mathf.DegToRad(axis));
            _spheres[i].Position = basis * basePosition;
        }
    }
}
