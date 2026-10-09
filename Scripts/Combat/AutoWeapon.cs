using Godot;
using SuperheroSurvivors.Core;

/// Базовый класс авто-оружия: поиск цели, нанесение урона с учётом
/// StatBlock героя (глобальный урон, крит). Новые оружия — наследники.
public abstract partial class AutoWeapon : Node3D
{
    public abstract string WeaponId { get; }

    protected new Player Owner => GameManager.Instance.Player;

    protected Enemy FindTarget() => GameManager.Instance.NearestEnemy(GlobalPosition);

    protected void DealDamage(Enemy enemy, float baseAmount, DamageType type)
    {
        if (enemy == null || !GodotObject.IsInstanceValid(enemy) || Owner == null) return;

        var stats = Owner.Stats;
        float amount = baseAmount * stats.Final(StatType.Damage);
        var info = DamageInfo.Auto(WeaponId, amount, type);

        if (GD.Randf() < stats.Final(StatType.CritChance))
            info = info.WithAmount(amount * stats.Final(StatType.CritMultiplier)).AsCrit();

        enemy.TakeDamages(info);
    }
}
