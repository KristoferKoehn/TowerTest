using Godot;
using System;

public partial class FileSelector : Control
{
	bool prev = false;
	[Export]
	ItemList files;
	[Export]
	Button LoadButton;

    [Signal]
    public delegate void LoadFileEventHandler(string file);



	bool MouseInside = false;
	bool dragging = false;
	Vector2 DragOffset = Vector2.Zero;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (prev != Visible) {
			LoadButton.Disabled = true;
			files.Clear();
            string[] names = DirAccess.GetFilesAt("res://Assets/WaveData/");
			foreach(string name in names)
			{
				files.AddItem(name);
            }
        }
		prev = Visible;


		if (dragging)
		{
			GlobalPosition = GetGlobalMousePosition() + DragOffset;
		}
	}

    public override void _Input(InputEvent @event)
    {
		if (@event.IsActionReleased("select"))
		{
			dragging = false;
		}


        if (@event.IsActionPressed("select") && MouseInside)
		{
            dragging = true;
			DragOffset = GlobalPosition - GetGlobalMousePosition();
		}
    }

    public void Close()
	{
		Visible = false;
	}

	public void _on_load_button_pressed()
	{
		EmitSignal("LoadFile", files.GetItemText(files.GetSelectedItems()[0]));
		Visible = false;
	}

    public void _on_item_list_item_selected(int index)
	{
		LoadButton.Disabled = false;
	}

	public void _on_panel_mouse_entered()
	{
		MouseInside = true;
	}

	public void _on_panel_mouse_exited()
	{
		MouseInside = false;
	}

}
