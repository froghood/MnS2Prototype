using Touhou.Objects.Characters;

namespace Touhou.Objects;

public enum CharacterOption : byte {

    [Character<Reimu>] Reimu,

    [Character<Marisa>] Marisa,

    [Character<Sakuya>] Sakuya,
}



static class CharacterOptionQuerier {
    public static Character GetCharacter(this CharacterOption option, params object[] args) {
        var type = typeof(CharacterOption);
        var name = Enum.GetName<CharacterOption>(option);

        var attribute = (CharacterAttribute)type.GetField(name).GetCustomAttributes(typeof(CharacterAttribute), false).FirstOrDefault();

        return attribute.GetCharacter(args);
    }
}

