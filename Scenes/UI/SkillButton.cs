using Godot;
using Godot.Collections;
using System;
using System.Linq;

public partial class SkillButton : TextureButton
{
    public Panel panel;
    public Label label;
    private ProgressBar progressBar;
    private Timer holdTimer;

    [Export]
    public float HoldDuration = 1f; // TotalDuration in seconds to hold the button

    [Export]
    Godot.Collections.Array<SkillButton> Prereqs = new Godot.Collections.Array<SkillButton>();

    // Calculated from prereqs of other buttons
    Dictionary<SkillButton, Line2D> NextSkillConnections = new Dictionary<SkillButton, Line2D>();

    private int _level;

    public int level
    {
        get { return _level; }
        set
        {
            _level = value;
            this.label.Text = _level.ToString() + "/3";
            UpdateVisualState();
        }
    }

    public override void _Ready()
    {
        this.panel = GetNode<Panel>("Panel");
        this.label = GetNode<Label>("MarginContainer/Label");
        this.progressBar = GetNode<ProgressBar>("ProgressBar");
        this.holdTimer = GetNode<Timer>("HoldTimer");
        this.ZIndex = 1;
        this.level = 0;
        UpdateVisualState();
        CallDeferred("CreateConnectingLines");

        this.holdTimer.WaitTime = HoldDuration;
        this.holdTimer.OneShot = true;
        this.holdTimer.Timeout += () => OnHoldTimerTimeout();
        this.progressBar.Visible = false;
    }

    private void CreateConnectingLines()
    {
        foreach (SkillButton prereqButton in this.Prereqs)
        {
            
            Line2D line = new Line2D();
            line.Width = this.Size.X / 16;
            Vector2 start = Size / 2;
            Vector2 end = (prereqButton.GlobalPosition + prereqButton.Size / 2) - GlobalPosition;
            line.AddPoint(start);
            line.AddPoint(end);
            AddChild(line);
            line.ZIndex = -1;
            prereqButton.NextSkillConnections.Add(this, line);
            
            /*
            Line2D line = new Line2D();
            AddChild(line);
            line.Width = this.Size.X / 16;
            Vector2 start = this.GlobalPosition + this.Size / 2; // middle of the button
            Vector2 end = prereqButton.GlobalPosition + prereqButton.Size / 2; // middle of the prereq button

            line.AddPoint(start);
            line.AddPoint(end);

            line.ZIndex = -1;
            prereqButton.NextSkillConnections.Add(this, line);
            */
        }
    }

    private void UpdateLinePositions()
    {
        foreach (SkillButton nextskill in this.NextSkillConnections.Keys)
        {
            Line2D line = this.NextSkillConnections[nextskill];
            line.ClearPoints();
            line.Width = this.Size.X / 16;
            Vector2 start = nextskill.Size / 2;
            Vector2 end = (this.GlobalPosition + this.Size / 2) - nextskill.GlobalPosition;
            line.AddPoint(start);
            line.AddPoint(end);
        }
    }


    public void _on_pressed()
    {
        // Do nothing here for now
    }

    public void _on_mouse_entered()
    {
        PackedScene tooltipScene = GD.Load<PackedScene>("res://Scenes/UI/SkillTooltip.tscn");
        PanelContainer tooltip = tooltipScene.Instantiate<PanelContainer>();
        this.AddChild(tooltip);
        tooltip.GlobalPosition = new Vector2(this.GlobalPosition.X + this.Size.X, this.GlobalPosition.Y);
        tooltip.ZIndex = 2;

        // Adjust position to ensure it fits on screen
        AdjustTooltipPosition(tooltip);
    }

    private void AdjustTooltipPosition(PanelContainer tooltip)
    {
        Vector2 screenSize = GetViewportRect().Size;
        Vector2 tooltipSize = tooltip.Size;

        Vector2 newPosition = tooltip.GlobalPosition;

        // Check if the tooltip is going off the right edge
        if (newPosition.X + tooltipSize.X > screenSize.X)
        {
            newPosition.X = screenSize.X - tooltipSize.X;
        }

        // Check if the tooltip is going off the left edge
        if (newPosition.X < 0)
        {
            newPosition.X = 0;
        }

        // Check if the tooltip is going off the bottom edge
        if (newPosition.Y + tooltipSize.Y > screenSize.Y)
        {
            newPosition.Y = screenSize.Y - tooltipSize.Y;
        }

        // Check if the tooltip is going off the top edge
        if (newPosition.Y < 0)
        {
            newPosition.Y = 0;
        }

        tooltip.GlobalPosition = newPosition;
    }


    public void _on_mouse_exited()
    {
        PanelContainer popup = GetNode<PanelContainer>("PanelContainer");
        popup.QueueFree();
    }

    public bool PrereqsMet()
    {
        foreach (SkillButton prereqButton in this.Prereqs)
        {
            if (prereqButton.level == 0)
            {
                return false;
            }
        }
        return true;
    }

    public void UpdateVisualState()
    {
        if (this.PrereqsMet())
        {
            this.Modulate = new Color(1, 1, 1, 1);
        }
        else
        {
            this.Modulate = new Color(0.5f, 0.5f, 0.5f, 1);
        }
    }

    private void OnHoldTimerTimeout()
    {
        this.level = (this.level + 1) % 4;
        this.panel.ShowBehindParent = true;
        foreach (SkillButton nextSkill in this.NextSkillConnections.Keys)
        {
            this.NextSkillConnections[nextSkill].DefaultColor = new Color(255,255,255);
            GD.Print("Changed line color");
            nextSkill.UpdateVisualState();
        }
        progressBar.Visible = false; // Hide progress bar after activation
    }

    public override void _Input(InputEvent @event)
    {
        if (!PrereqsMet()) return;

        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
            {
                if (this.GetGlobalRect().HasPoint(this.GetGlobalMousePosition()))
                {
                    progressBar.Value = 0;
                    progressBar.Visible = true;
                    holdTimer.Start();
                }
            }
            else if (!mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
            {
                progressBar.Visible = false;
                holdTimer.Stop();
            }
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        progressBar.Value = (1 - (holdTimer.TimeLeft / HoldDuration)) * 100;
        UpdateLinePositions();

    }
}
