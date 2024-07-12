using Godot;
using System;

public partial class SkillTree : Control
{
    private Camera2D _camera;
    private Vector2 _lastMousePosition;
    private bool _dragging = false;

    [Export]
    public float ZoomSpeed = 0.1f;
    [Export]
    public float MinZoom = 0.5f;
    [Export]
    public float MaxZoom = 2.0f;


    public override void _Ready()
    {
        this._camera = GetNode<Camera2D>("Camera2D");
    }

    public override void _Input(InputEvent @event)
    {
        // Handle zooming
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.WheelUp && mouseEvent.Pressed)
            {
                _camera.Zoom *= (1 - ZoomSpeed);
                _camera.Zoom = new Vector2(Mathf.Clamp(_camera.Zoom.X, MinZoom, MaxZoom), Mathf.Clamp(_camera.Zoom.Y, MinZoom, MaxZoom));
            }
            else if (mouseEvent.ButtonIndex == MouseButton.WheelDown && mouseEvent.Pressed)
            {
                _camera.Zoom *= (1 + ZoomSpeed);
                _camera.Zoom = new Vector2(Mathf.Clamp(_camera.Zoom.X, MinZoom, MaxZoom), Mathf.Clamp(_camera.Zoom.Y, MinZoom, MaxZoom));
            }
        }

        // Handle dragging
        if (@event is InputEventMouseMotion mouseMotionEvent)
        {
            if (_dragging)
            {
                Vector2 delta = mouseMotionEvent.GlobalPosition - _lastMousePosition;
                _camera.Position -= delta / _camera.Zoom;  // Adjust by the zoom level
                _lastMousePosition = mouseMotionEvent.GlobalPosition;
            }
        }

        if (@event is InputEventMouseButton mouseButtonEvent)
        {
            if (mouseButtonEvent.ButtonIndex == MouseButton.Left)
            {
                if (mouseButtonEvent.Pressed)
                {
                    _dragging = true;
                    _lastMousePosition = mouseButtonEvent.GlobalPosition;
                }
                else
                {
                    _dragging = false;
                }
            }
        }
    }
}
