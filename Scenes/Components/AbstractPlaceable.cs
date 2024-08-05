using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TowerTest.Scenes.Components
{
    public abstract partial class AbstractPlaceable : Node3D
    {

        [Signal]
        public delegate void PlacedEventHandler(Node3D item, Vector3 position);

        [Signal]
        public delegate void CancelEventHandler(Node3D item = null);
        public abstract void DisplayMode();
        public abstract void Activate();


        public override void _EnterTree()
        {
            base._EnterTree();
        }

        public override void _ExitTree()
        {
            base._ExitTree();
        }

        public override void _Ready()
        {
            base._Ready();
        }

        public override void _Process(double delta)
        {
            base._Process(delta);
        }

    }
}
