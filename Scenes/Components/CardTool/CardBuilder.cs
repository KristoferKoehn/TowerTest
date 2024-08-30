using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class CardBuilder : Node3D
{
    [Signal] public delegate void ValuesUpdatedEventHandler();
    [Export] LineEdit NameEdit { get; set; }
	[Export] Card SmallCard { get; set; }
    [Export] Card TiltedCard { get; set; }
    [Export] Card BigCard { get; set; }
    [Export] ItemList ItemList { get; set; }
    [Export] OptionButton SubjectTypeSelector { get; set; }
    [Export] TextEdit TextEdit { get; set; }
    [Export] OptionButton RarityOptionButton { get; set; }
    [Export] OptionButton ResourceTypeOptionButton {  get; set; }
    [Export] SpinBox ResourceCostSpinbox { get; set; }
    [Export] FileSelector FileSelector { get; set; }
    [Export] SpinBox ShopCostSpinBox { get; set; }
    [Export] HSlider OrbitSlider {  get; set; }
    [Export] HSlider TiltSlider {  get; set; }
    [Export] HSlider ZoomSlider {  get; set; }
    [Export] HSlider FOVSlider {  get; set; }
    [Export] TextEdit CardInfoTextEdit { get; set; }

    CardData cd = new();
    string subject = null;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        AddChild(CardLoadingManager.GetInstance());
        _on_option_button_item_selected(0);

        foreach (StatType st in Enum.GetValues(typeof(StatType)))
        {
            ResourceTypeOptionButton.AddItem(st.ToString());
        }
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}

	public void _on_option_button_item_selected(int idx)
	{
        ItemList.Clear();


        string dir = SubjectTypeSelector.GetItemText(idx);

        string[] names = DirAccess.GetFilesAt($"res://Scenes/{dir}/");
        foreach (string name in names)
        {
            if (name.Contains(".tscn"))
            {
                ItemList.AddItem(name);
            }
        }
    }

    public void _on_item_list_item_clicked(int idx, Vector2 pos, int mousebuttonidx)
    {
        string item = ItemList.GetItemText(idx);
        if (item != subject && mousebuttonidx == 1)
        {
            subject = item;
            cd.SubjectScene = subject;
            EmitSignal("ValuesUpdated");
        }
        
    }

    public void _on_orbit_slider_value_changed(float value)
    {
        cd.CameraOrbitSpeed = value;
        BigCard.Viewport.CameraOrbitSpeed = value;
        SmallCard.Viewport.CameraOrbitSpeed = value;
        TiltedCard.Viewport.CameraOrbitSpeed = value;

    }

    public void _on_tilt_slider_value_changed(float value)
    {
        cd.CameraTilt = value;
        BigCard.Viewport.CameraTilt = value;
        SmallCard.Viewport.CameraTilt = value;
        TiltedCard.Viewport.CameraTilt = value;
    }

    public void _on_zoom_slider_value_changed(float value)
    {
        cd.CameraZoom = value;
        BigCard.Viewport.CameraZoom = value;
        SmallCard.Viewport.CameraZoom = value;
        TiltedCard.Viewport.CameraZoom = value;
    }

    public void _on_fov_slider_value_changed(float value)
    {
        cd.FOV = value;
        BigCard.Viewport.Camera.Fov = value;
        SmallCard.Viewport.Camera.Fov = value;
        TiltedCard.Viewport.Camera.Fov = value;
    }

    public void _on_text_edit_text_changed()
    {
        cd.CardInfo = TextEdit.Text;
    }

    public void _on_line_edit_text_changed(string text)
    {
        cd.Name = text;
        BigCard.CardNameLabel.Text = text;
        SmallCard.CardNameLabel.Text = text;
        TiltedCard.CardNameLabel.Text = text;
    }

    public void _on_refresh_button_pressed()
    {
        EmitSignal("ValuesUpdated");
    }

    public void _on_rarity_option_button_item_selected(int idx)
    {
        string rarity = RarityOptionButton.GetItemText(idx);
        Enum.TryParse(rarity, out Rarity cr);
        cd.Rarity = cr;
        EmitSignal("ValuesUpdated");
    }

    public void _on_resource_type_option_box_item_selected(int idx)
    {
        StatType st = (StatType)Enum.Parse(typeof(StatType), ResourceTypeOptionButton.GetItemText(idx));
        cd.ResourceCostType = st;
    }

    public void _on_resource_cost_spinbox_value_changed(float val )
    {
        cd.ResourceCostValue = val;
    }


    public void _on_values_updated()
    {
        BigCard.SetCardData(cd);
        SmallCard.SetCardData(cd);
        TiltedCard.SetCardData(cd);
    }

    public void UpdateEditor()
    {
        NameEdit.Text = cd.Name;
        ShopCostSpinBox.Value = cd.ShopCost;
        Array list = Enum.GetValues(typeof(StatType));
        int idx = 0;
        foreach (StatType st in list)
        {
            if (st == cd.ResourceCostType)
            {
                ResourceTypeOptionButton.Select(idx);
            }
            idx++;
        }

        ResourceCostSpinbox.Value = cd.ResourceCostValue;
        for (int i = 0; i < RarityOptionButton.ItemCount; i++)
        {
            string rarity = RarityOptionButton.GetItemText(idx);
            Enum.TryParse(rarity, out Rarity cr);

            if (cr == cd.Rarity)
            {
                RarityOptionButton.Select(i);
            }
        }

        OrbitSlider.Value = cd.CameraOrbitSpeed;
        TiltSlider.Value = cd.CameraTilt;
        ZoomSlider.Value = cd.CameraZoom;
        FOVSlider.Value = cd.FOV;

        CardInfoTextEdit.Text = cd.CardInfo;

    }

    public void _on_save_button_pressed()
    {
        string[] CardNames = DirAccess.GetFilesAt("res://Scenes/CardData/");

        bool UniqueNameFound = false;
        int idx = 0;
        while (!UniqueNameFound)
        {
            if (!CardNames.Contains(idx == 0 ? $"{cd.Name}.tres" : $"{cd.Name}{idx}.tres"))
            {
                UniqueNameFound = true;
            } else
            {
                idx++;
            }
            
        }
        

        ResourceSaver.Save(cd, idx == 0 ? $"res://Scenes/CardData/{cd.Name}.tres" : $"res://Scenes/CardData/{cd.Name}{idx}.tres");

    }

    public void _on_load_button_pressed()
    {
        FileSelector.Visible = true;
    }

    public void _on_file_selector_load_file(string text)
    {
        cd = (CardData)ResourceLoader.Load($"res://Scenes/CardData/{text}");
        EmitSignal("ValuesUpdated");
        UpdateEditor();
    }

}
