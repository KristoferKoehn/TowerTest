using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class WaveMaker : Node3D
{

    Array<Array<Array<string>>> Level = new Array<Array<Array<string>>>();
    [Export]
    ItemList WaveItemList;
    [Export]
    ItemList ForkItemList;
    [Export]
    OptionButton OptionButton;
    [Export]
    Button AddFork;
    [Export]
    Button AddWave;
    [Export]
    Button RemoveEnemy;
    [Export]
    MenuButton Menu;
    [Export]
    FileSelector FileSelector;
    [Export]
    Button SaveButton;
    [Export]
    LineEdit LineEdit;


    List<ViewportVisuals> Viewports = new List<ViewportVisuals>();


    int CurrentWave = 0;
    //select wave
    //select enemy to add to wave
    //crack open a viewport scene and put that enemy in it, create an item in the itemlist to stream the texture to. The Itemlist will show all the enemies in there


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        RemoveEnemy.Disabled = true;
        InitializeLevel();
        _on_spin_box_value_changed(1);
        Menu.GetPopup().IndexPressed += MenuButtonPressed;

        //put enemy scene names into a dropdown or something
        string[] names = DirAccess.GetFilesAt("res://Scenes/Enemies/");
        List<string> scenes = new List<string>();
        foreach (string name in names)
        {
            if (name.Contains(".tscn"))
            {
                scenes.Add(name);
            }
        }
        foreach (string name in scenes) {
            OptionButton.AddItem(name);
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {

    }

    public void Save(string content, string LevelName)
    {
        using var file = FileAccess.Open($"res://Assets/WaveData/{LevelName}", FileAccess.ModeFlags.Write);
        file.StoreString(content);
    }

    public void _on_add_button_pressed()
    {
        string scene = OptionButton.GetPopup().GetItemText(OptionButton.Selected);
        AddToItemList(WaveItemList, scene);
        Level[CurrentWave][0].Add(scene);

    }

    public void _on_add_fork_button_pressed()
    {

        string scene = OptionButton.GetPopup().GetItemText(OptionButton.Selected);
        AddToItemList(ForkItemList, scene);
        Level[CurrentWave][1].Add(scene);

    }


    public void _on_standard_wave_list_item_selected(int index)
    {
        RemoveEnemy.Disabled = false;
        ForkItemList.DeselectAll();
    }

    public void _on_fork_wave_list_item_selected(int index)
    {
        RemoveEnemy.Disabled = false;
        WaveItemList.DeselectAll();
    }

    public void _on_spin_box_value_changed(int value)
    {

        ForkItemList.DeselectAll();
        ForkItemList.Clear();
        WaveItemList.DeselectAll();
        WaveItemList.Clear();
        foreach(ViewportVisuals vv in Viewports)
        {
            vv.QueueFree();
        }
        Viewports.Clear();

        RemoveEnemy.Disabled = true;

        if (Level.Count < value - 1)
        {
            for (int i = Level.Count; i > value - 1; i++)
            {
                Level[i].Add(new Array<string>());
                Level[i].Add(new Array<string>());
            }
        }
        CurrentWave = value - 1;
        if (Level[CurrentWave] == null)
        {
            Level[CurrentWave].Add(new Array<string>());
            Level[CurrentWave].Add(new Array<string>());
        }

        foreach(string scene in Level[CurrentWave][0])
        {
            AddToItemList(WaveItemList, scene);
        }

        foreach (string scene in Level[CurrentWave][1])
        {
            AddToItemList(ForkItemList, scene);
        }

    }

    public void _on_remove_button_pressed()
    {
        int[] ForkSelection = ForkItemList.GetSelectedItems();
        if (ForkSelection != null && ForkSelection.Length > 0)
        {
            foreach(int i in ForkSelection)
            {
                ForkItemList.RemoveItem(i);
                Level[CurrentWave][1].Remove(Level[CurrentWave][1][i]);
                if (i-1 > -1)
                {
                    ForkItemList.Select(i);
                }
                else if (i - 1 < 0)
                {
                    //do not select
                }
                else if (i > Level[CurrentWave][1].Count - 1)
                {
                    ForkItemList.Select(Level[CurrentWave][1].Count - 1);
                }
            }
        }

        int[] WaveSelection = WaveItemList.GetSelectedItems();
        if (WaveSelection != null && WaveSelection.Length > 0)
        {
            foreach (int i in WaveSelection)
            {
                WaveItemList.RemoveItem(i);
                Level[CurrentWave][0].Remove(Level[CurrentWave][0][i]);
                if (i < Level[CurrentWave][0].Count)
                {
                    WaveItemList.Select(i);
                } else if (i - 1 < 0)
                {
                    //do not select
                } else if (i > Level[CurrentWave][0].Count - 1)
                {
                    WaveItemList.Select(Level[CurrentWave][0].Count - 1);
                } 
            }
        }
    }

    private void InitializeLevel()
    {
        for(int i = 0; i < 50; i++)
        {
            Level.Add(new Array<Array<string>>());
            Level[i].Add(new Array<string>());
            Level[i].Add(new Array<string>());
        }
    }

    public void _on_fork_wave_list_multi_selected(int index, bool selected)
    {
        RemoveEnemy.Disabled = false;
        WaveItemList.DeselectAll();
    }

    public void _on_standard_wave_list_multi_selected(int index, bool selected)
    {
        RemoveEnemy.Disabled = false;
        ForkItemList.DeselectAll();
    }

    public void AddToItemList(ItemList itemList, string scene)
    {
        PackedScene enemy = GD.Load<PackedScene>($"res://Scenes/Enemies/{scene}");
        ViewportVisuals vv = GD.Load<PackedScene>("res://Scenes/Utility/ViewportVisuals.tscn").Instantiate<ViewportVisuals>();
        vv.SubjectPackedScene = enemy;
        Viewports.Add(vv);
        AddChild(vv);
        Texture2D t = vv.GetTexture();
        vv.CameraZoom = 2;
        vv.CameraTilt = -23;
        itemList.AddIconItem(t);
    }

    public void MenuButtonPressed(long index)
    {
        if (index == 0)
        {
            FileSelector.Visible = true;
        }
    }

    public void _on_file_selector_load_file(string filename)
    {
        string json = Load(filename);
        filename = filename.ReplaceN(".json", "");
        LineEdit.Text = filename;
        Variant v = Json.ParseString(json);
        Level = v.AsGodotArray<Array<Array<string>>>();
        _on_spin_box_value_changed(1);
    }

    public string Load(string fileName)
    {
        using var file = FileAccess.Open($"res://Assets/WaveData/{fileName}", FileAccess.ModeFlags.Read);
        if (file == null)
        {
            return null;
        }
        string content = file.GetAsText();
        return content;
    }

    public void _on_save_button_pressed()
    {
        
        string str = Json.Stringify(Level, indent: " ");
        Save(str, $"{LineEdit.Text}.json");
    }



}
