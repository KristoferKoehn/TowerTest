using Godot;
using System;

public partial class DamageNumbers : Node
{
    private static DamageNumbers instance;

    public static DamageNumbers GetInstance()
    {
        if (instance == null)
        {
            instance = new DamageNumbers();
            SceneSwitcher.root.GetNode<SceneSwitcher>("SceneSwitcher").AddChild(instance);
            instance.Name = "DamageNumbers";
        }
        return instance;
    }

    public async void DisplayDamageNumbers(float damage, Vector2 position, bool isCritical)
    {
        GD.Print("displaying damage number at " + position.X + ", " + position.Y);
        Label numberLabel = new Label();
        numberLabel.GlobalPosition = position;
        numberLabel.Text = damage.ToString();
        numberLabel.ZIndex = 5;

        // Setting label properties
        Color color = new Color("#FFF");
        if (isCritical)
        {
            color = new Color("#B22");
        }
        if (damage == 0)
        {
            color = new Color("#FFF8");
        }
        numberLabel.Modulate = color;
        numberLabel.Set("custom_fonts/font_size", 18);
        numberLabel.Set("custom_fonts/outline_color", new Color("#000"));
        numberLabel.Set("custom_fonts/outline_size", 1);

        // Add the label to the scene tree
        CallDeferred("add_child", numberLabel);

        // Wait for the label to resize (asynchronous operation)
        await ToSignal(numberLabel, "resized");
        numberLabel.PivotOffset = numberLabel.Size / 2;

        // Create and configure the tween
        Tween tween = GetTree().CreateTween();
        tween.SetParallel(true);
        tween.TweenProperty(numberLabel, "position:y", numberLabel.Position.Y - 24, 0.25f).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(numberLabel, "position:y", numberLabel.Position.Y, 0.5f).SetEase(Tween.EaseType.In).SetDelay(0.25f);
        tween.TweenProperty(numberLabel, "scale", Vector2.Zero, 0.25f).SetEase(Tween.EaseType.In).SetDelay(0.5f);

        // Wait for the tween to finish
        await ToSignal(tween, "finished");
        numberLabel.QueueFree();
    }
}
