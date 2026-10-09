using System;

/// Один вариант на выбор при левел-апе («1 из 4», модель Brotato).
public class LevelUpOption
{
    public string Title { get; }
    public string Description { get; }
    public Action<Player> Apply { get; }

    public LevelUpOption(string title, string description, Action<Player> apply)
    {
        Title = title;
        Description = description;
        Apply = apply;
    }
}
