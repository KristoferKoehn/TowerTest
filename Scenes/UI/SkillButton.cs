using Godot;
using Godot.Collections;
using System;

public partial class SkillButton : TextureButton
{
    public Panel panel;
    public Label label;

    [Export]
    Godot.Collections.Array<SkillButton> Prereqs = new Godot.Collections.Array<SkillButton>();

    // A dictionary of the next skill buttons (ones that you unlock after having the current button unlocked) and a line connecting.
    Dictionary<SkillButton, Line2D> NextSkillConnections = new Dictionary<SkillButton, Line2D>();

    // Backing field for the level property
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

    public void _on_pressed()
    {
        if (!PrereqsMet()) // Do nothing
        {
            return;
        }

        // If prereqs have been met:
        this.level = (this.level + 1) % 4;
        this.panel.ShowBehindParent = true;
        foreach (SkillButton nextSkill in this.NextSkillConnections.Keys)
        {
            // Coloring the line:
            this.NextSkillConnections[nextSkill].DefaultColor = new Color(1, 1, 0.24705882370472f);
            // Updating the other buttons (so that they will show they can now be unlocked)
            nextSkill.UpdateVisualState();
        }
    }

    // Method to check if all prerequisites are met
    public bool PrereqsMet()
    {
        foreach (SkillButton prereqButton in this.Prereqs)
        {
            // The skills are only unlocked if all the prereqs have been unlocked to at least 1.
            // This can be changed if we want them to have to unlock the skill to level 3 to get the next skill.
            if (prereqButton.level == 0)
            {
                return false;
            }
        }
        return true;
    }

    // Method to update the visual state of the button
    public void UpdateVisualState()
    {
        if (this.PrereqsMet())
        {
            // Light up the button to show it is ready to be unlocked
            this.Modulate = new Color(1, 1, 1, 1); // Full brightness (white glow)
        }
        else
        {
            // Reset the button's modulate property
            this.Modulate = new Color(0.5f, 0.5f, 0.5f, 1); // Dimmed (grayish)
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.panel = GetNode<Panel>("Panel");
        this.label = GetNode<Label>("MarginContainer/Label");
        this.ZIndex = 1;

        foreach (SkillButton prereqButton in this.Prereqs)
        {
            Line2D line = new Line2D();

            // Calculate positions relative to parent node
            Vector2 start = Size / 2;  // Adjust this if needed
            Vector2 end = (prereqButton.Position + prereqButton.Size / 2) - Position;

            line.AddPoint(start);
            line.AddPoint(end);

            // Add the line to the parent node
            AddChild(line);
            line.ZIndex = -1;

            // Store the line in the dictionary for later reference
            prereqButton.NextSkillConnections.Add(this, line);
        }

        // Initialize the level to 0 or any other default value
        this.level = 0;

        // Update the visual state of the button
        UpdateVisualState();
    }
}