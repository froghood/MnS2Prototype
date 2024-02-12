namespace Touhou.Objects.Characters;

[AttributeUsage(AttributeTargets.Field)]
sealed class CharacterAttribute<T> : CharacterAttribute where T : Character {
    public override Character GetCharacter(params object[] args) {
        return (Character)Activator.CreateInstance(typeof(T), args);
    }
}

public abstract class CharacterAttribute : Attribute {


    public abstract Character GetCharacter(params object[] args);
}

