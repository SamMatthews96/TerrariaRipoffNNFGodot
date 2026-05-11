// using System;
// using Godot;
//
// namespace TerrariaRipoffNNF.Scripts.GameObjects.WeaponSprites;
//
// public partial class WeaponAttackSwing : WeaponAttackNode {
//     private float _rotationSpeed = (float)Math.PI * 1.5f;
//     private float _swingSize = (float)Math.PI * 0.7f;
//     private int _swingDirection;
//     [Export] private Timer _timer;
//     private Vector2 _centerOffset = new(22, -50);
//
//     public override void _Ready() {
//         Position = Player.Position + _centerOffset;
//
//         Vector2 delta = TargetPosition - Position;
//         float swingMiddle = (float)Math.Atan2(delta.Y, delta.X);
//         bool isFacingLeft = Math.PI / 2 < Math.Abs(swingMiddle);
//         _swingDirection = isFacingLeft ? -1 : 1;
//         Rotation = swingMiddle - _swingDirection * _swingSize / 2;
//         
//         _timer.WaitTime = _swingSize / _rotationSpeed;
//         _timer.Start();
//         _timer.Timeout += OnTimeout;
//     }
//
//     private void OnTimeout() {
//         QueueFree();
//     }
//
//     public override void _PhysicsProcess(double delta) {
//         Position = Player.Position + _centerOffset;
//         Rotation += _rotationSpeed * _swingDirection * (float)delta;
//     }
// }