using Godot;
using Godot.Collections;

[GlobalClass]
public partial class DeckData : Resource
{
    [Export] public Array<CardData> data = new();
}
