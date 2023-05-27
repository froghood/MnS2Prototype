using System.Net;
using SFML.Graphics;
using SFML.System;
using Touhou.Net;
using Touhou.Objects;

namespace Touhou.Objects.Characters;

public class OpponentReimu : Opponent {

    public OpponentReimu(Vector2f startingPosition) : base(startingPosition) {

        AddAttack(PacketType.Primary, new ReimuPrimary());
        AddAttack(PacketType.Secondary, new ReimuSecondary());
        AddAttack(PacketType.SpellA, new ReimuSpellA());
        AddAttack(PacketType.SpellB, new ReimuSpellB());
    }
}