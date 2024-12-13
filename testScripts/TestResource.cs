using System;
using Godot;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class TestResource : Resource {
    [Export] private string _testString;
}