using Godot;
using Godot.Collections;

public partial class DraggerButton : Button
{
    [Export]
    Texture2D ButtonIcon;
    [Export]
    PackedScene SpawnScene;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.Icon = ButtonIcon;

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public override void _PhysicsProcess(double delta)
    {

    }


    public void _on_button_down()
    {

    }

    public void _on_button_up()
    {

    }

    public void _on_pressed()
    {
        AbstractTower tower = SpawnScene.Instantiate<AbstractTower>();
        tower.Placing = true;
        SceneSwitcher.root.GetNode<GameLoop>("SceneSwitcher/GameLoop").AddChild(tower);
    }

}