using Godot;
using Godot.Collections;
using System;

public partial class SkillButton : TextureButton
{
    public Panel panel;
    public Label label;

    [Export]
    Godot.Collections.Array<SkillButton> Prereqs = new Godot.Collections.Array<SkillButton>();

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
        }
    }

    public void _on_pressed()
    {
        if (!PrereqsMet())
        {
            return;
        }

        this.level = (this.level + 1) % 4;
        this.panel.ShowBehindParent = true;
        foreach(SkillButton nextSkill in this.NextSkillConnections.Keys)
        {
            this.NextSkillConnections[nextSkill].DefaultColor = new Color(1, 1, 0.24705882370472f);
        }
    }

    // Method to check if all prerequisites are met
    public bool PrereqsMet()
    {
        foreach (SkillButton prereqButton in this.Prereqs)
        {
            if (prereqButton.level == 0) // As long as the prereq skills are level 1
            {
                return false;
            }
        }
        return true;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.panel = GetNode<Panel>("Panel");
        this.label = GetNode<Label>("MarginContainer/Label");

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
            line.ShowBehindParent = true;

            // Store the line in the dictionary for later reference
            prereqButton.NextSkillConnections.Add(this, line);
        }

        // Initialize the level to 0 or any other default value
        this.level = 0;
    }

}