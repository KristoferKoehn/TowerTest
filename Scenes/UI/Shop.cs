using Godot;
using System;
using Godot.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public partial class Shop : Control
{
	//either through UI or this, make shop inaccessible during wave
	//on end of wave, randomize cards using some heuristic later.
	//differentiate common/uncommon and rare/epic/legendary slots.
	//we can dynamically add card slots that's not a big deal. Maybe for upgrading the shop somehow
	[Signal] public delegate void ShopClosedEventHandler();

    //    [Signal]
    //public delegate void DragEndedEventHandler(Card sender);


    [Export] Array<Card> Cards;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		List<CardData> data = GetRandomDataList();


		for (int i = 0; i < Cards.Count; i++)
		{
			Cards[i].SetCardData(data[i]);
		}
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}

	public List<CardData> GetRandomDataList()
	{
		List<CardData> list = CardLoadingManager.GetInstance().GetAllCardData();
		List<CardData> ret = new List<CardData>();
		for (int i = 0; i < 7; i++)
		{
			ret.Add(list[Mathf.Abs((int)RNGManager.GetInstance().rng.Randi()) % list.Count]);
		}
		return ret;
    }

	public void _on_close_button_pressed()
	{
		this.Visible = false;
		EmitSignal("ShopClosed");
	}

}
