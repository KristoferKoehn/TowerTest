using Godot;
using System;

public partial class PauseMenu : Control
{
    // Pause Menu Stuff/Buttons:

    [Export] private ConfirmationDialog returnToTitleConfirmationDialog;
    [Export] private ConfirmationDialog quitConfirmationDialog;

    public override void _Ready()
    {
        returnToTitleConfirmationDialog.Confirmed += () => OnConfirmReturnToTitle();
        quitConfirmationDialog.Confirmed += () => OnConfirmQuit();
        this.Visible = false;
    }

    public void _on_resume_pressed()
    {
        GetTree().Paused = !GetTree().Paused;
        this.Visible = !this.Visible;
    }

    public void _on_return_to_title_screen_pressed()
    {
        returnToTitleConfirmationDialog.Show();
        returnToTitleConfirmationDialog.PopupCentered();
    }

    public void _on_quit_pressed()
    {
        quitConfirmationDialog.Show();
        quitConfirmationDialog.PopupCentered();
    }

    private void OnConfirmReturnToTitle()
    {
        GetTree().Paused = false;
        Engine.TimeScale = 1;
        SceneSwitcher.Instance.PopScene();
    }

    private void OnConfirmQuit()
    {
        GetTree().Quit();
    }
}
