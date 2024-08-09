using Godot;
using System;

public partial class DamageNumbers : Node
{
    public async void DisplayDamageNumbers(int damage, Vector2 position, bool isCritical)
    {
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
        CallDeferred("AddChild", numberLabel);

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
