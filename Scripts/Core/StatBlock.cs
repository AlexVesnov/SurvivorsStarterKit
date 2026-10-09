using System.Collections.Generic;

namespace SuperheroSurvivors.Core;

/// Блок статов героя: базовые значения + модификаторы с пересчётом.
/// Чистая логика без Godot — юнит-тестируется. Ошибка кита «+1 в свитче»
/// заменена единой формулой: итог = (база + Σ аддитивных) × Π мультипликативных.
public class StatBlock
{
    private readonly Dictionary<StatType, float> _base = new();
    private readonly Dictionary<StatType, List<StatModifier>> _modifiers = new();

    public void SetBase(StatType stat, float value) => _base[stat] = value;

    public float GetBase(StatType stat) => _base.TryGetValue(stat, out float v) ? v : 0f;

    public void AddModifier(StatType stat, StatModifier modifier)
    {
        if (!_modifiers.TryGetValue(stat, out var list))
        {
            list = new List<StatModifier>();
            _modifiers[stat] = list;
        }
        list.Add(modifier);
    }

    public bool RemoveModifiersFrom(string sourceId)
    {
        bool removed = false;
        foreach (var list in _modifiers.Values)
            removed |= list.RemoveAll(m => m.SourceId == sourceId) > 0;
        return removed;
    }

    public float Final(StatType stat)
    {
        float final = GetBase(stat);
        if (!_modifiers.TryGetValue(stat, out var list)) return final;

        float additive = 0f;
        float multiplicative = 1f;
        foreach (var m in list)
        {
            additive += m.Additive;
            multiplicative *= m.Multiplicative;
        }
        return (final + additive) * multiplicative;
    }
}
