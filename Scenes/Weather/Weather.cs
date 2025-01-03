using Godot;
using Godot.Collections;
using System.Collections.Generic;
using TowerTest.Scenes.Components;

public enum WeatherType
{
    Rain,
    Heatwave,
    Storm,
    Snow,
    Hail,
}

// Weather is like a mesh with a area3d that determines when enemies are in it,
// then effect them somehow based on the type of weather.
public partial class Weather : AbstractPlaceable
{
    [Export] private MeshInstance3D mesh;
    [Export] private Area3D area3D;
    [Export] private CollisionShape3D collisionShape3D;
    [Export] private Timer damageTimer;
    [Export] private Timer durationTimer;
    [Export] private bool Disabled = false;
    [Export] public bool Placing = false;
    [Export] public bool Valid = false;
    [Export] public StandardMaterial3D InvalidMaterial;
    [Export] public StandardMaterial3D ValidMaterial;
    private Vector3 PlaceSpot = Vector3.Zero;
    private MeshInstance3D currentTile = null;
    private bool PrevPlacing = false;
    private MeshInstance3D indicator { get; set; }
    private HashSet<BaseEnemy> enemiesInWeather = new HashSet<BaseEnemy>();

    internal List<PackedScene> statusEffectsToApply = new List<PackedScene>();
    internal float damageAmount;
    internal float damageInterval;
    internal WeatherType weatherType { get; set; }
    internal int durationSeconds;
    internal DamageType damageType;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

        this.damageTimer.OneShot = false;
        this.damageTimer.WaitTime = damageInterval;

        this.durationTimer.OneShot = true;
        this.durationTimer.WaitTime = durationSeconds;

        this.area3D.AreaEntered += RegisterEnemy;
        this.area3D.AreaExited += DeregisterEnemy;
        damageTimer.Timeout += ApplyDamage;
        this.durationTimer.Timeout += () => this.QueueFree();
    }

    public override void _Process(double delta)
    {
        if (Disabled) return;

        if (!PrevPlacing && Placing)
        {
            indicator = new MeshInstance3D();
            QuadMesh q = new QuadMesh();
            q.Orientation = PlaneMesh.OrientationEnum.Y;
            indicator.Mesh = q;
            //SelectorHitbox.GetNode<CollisionShape3D>("CollisionShape3D").Disabled = true;
            AddChild(indicator);
        }

        if (Placing)
        {
            //get raycast
            var SpaceState = GetWorld3D().DirectSpaceState;
            Vector2 MousePos = GetViewport().GetMousePosition();
            Vector3 Origin = GetViewport().GetCamera3D().ProjectRayOrigin(MousePos);
            Vector3 End = Origin + GetViewport().GetCamera3D().ProjectRayNormal(MousePos) * 1000f;

            var Query = PhysicsRayQueryParameters3D.Create(Origin, End);
            Dictionary RayResult = SpaceState.IntersectRay(Query);

            if (RayResult.Count > 0 && ((Node3D)RayResult["collider"]).GetParent() != this)
            {
                Tween t = GetTree().CreateTween();

                Vector3 pos = (Vector3)RayResult["position"];
                pos = new Vector3(Mathf.Round(pos.X), pos.Y, Mathf.Round(pos.Z));
                PlaceSpot = new Vector3(pos.X, this.Position.Y, pos.Z);
                t.TweenProperty(this, "global_position", pos + new Vector3(0, 0, 0), 0.1);

                indicator.GlobalPosition = pos + new Vector3(0, 0.1f, 0);

                MeshInstance3D tile = ((Node3D)RayResult["collider"]).GetParent() as MeshInstance3D;
                if (tile != null && tile.HasMeta("height") && !tile.HasMeta("tile_invalid"))
                {
                    currentTile = tile;
                    if (tile.GetMeta("height").AsInt32() > 0)
                    {
                        Valid = true;
                        SetChildMaterialOverride(this, ValidMaterial);
                    }
                    else
                    {
                        Valid = false;
                        SetChildMaterialOverride(this, InvalidMaterial);
                    }

                }
                else
                {
                    Valid = false;
                    SetChildMaterialOverride(this, InvalidMaterial);
                    currentTile = null;
                }
            }

            //placement
            if (Input.IsActionJustReleased("select"))
            {
                if (Valid && currentTile != null)
                {
                    SetChildMaterialOverride(this, null);
                    indicator.Visible = false;
                    Tween t = CreateTween();
                    t.TweenProperty(this, "global_position", PlaceSpot, 0.1);
                    Placing = false;
                    //SelectorHitbox.GetNode<CollisionShape3D>("CollisionShape3D").Disabled = false;
                    currentTile.SetMeta("tile_invalid", true);
                    EmitSignal("Placed", this, PlaceSpot);

                    this.damageTimer.Start();
                    this.durationTimer.Start();
                }
            }

            if (Input.IsActionJustPressed("cancel"))
            {
                this.QueueFree();
                EmitSignal("Cancelled");
            }
        }

        PrevPlacing = Placing;
    }

    // Apply an effect or stat change when they are in the weather (damage, slow, etc.)
    private void RegisterEnemy(Node body)
    {
        if (body.GetParent() is BaseEnemy enemy)
        {
            enemiesInWeather.Add(enemy);

            foreach (PackedScene effect in statusEffectsToApply)
            {
                BaseStatusEffect effectscene = effect.Instantiate<BaseStatusEffect>();
                enemy.AddStatusEffect(effectscene);
            }
        }
    }

    // If an enemy was slowed, reset their stats back when they leave the weather
    private void DeregisterEnemy(Node body)
    {
        if (body.GetParent() is BaseEnemy enemy)
        {
            enemiesInWeather.Remove(enemy);
        }
    }

    private void ApplyDamage()
    {
        if (enemiesInWeather.Count > 0)
        {
            foreach (BaseEnemy enemy in enemiesInWeather)
            {
                if (!IsInstanceValid(enemy))
                {
                    enemiesInWeather.Remove(enemy);
                }
                else
                {
                    enemy.TakeDamage(damageAmount, this, false, damageType);
                }
            }
        }
    }

    public override void DisplayMode()
    {
        this.Disabled = true;
    }

    public override void ActivatePlacing()
    {
        GlobalPosition = SceneSwitcher.CurrentGameLoop.MousePosition3D;
        Disabled = false;
        Placing = true;
    }

    public void SetChildMaterialOverride(Node node, StandardMaterial3D sm3d)
    {
        foreach (Node child in node.GetChildren())
        {
            if (child is MeshInstance3D mesh)
            {
                SetMeshMaterialOverride(mesh, sm3d);
            }

            if (child.GetChildCount() > 0)
            {
                SetChildMaterialOverride(child, sm3d);
            }
        }
    }

    public void SetMeshMaterialOverride(MeshInstance3D mesh, StandardMaterial3D sm3d)
    {
        for (int i = 0; i < mesh.GetSurfaceOverrideMaterialCount(); i++)
        {
            mesh.SetSurfaceOverrideMaterial(i, sm3d);
        }
    }
}