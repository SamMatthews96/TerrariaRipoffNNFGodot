using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class Breakables : Resource {
    [Export] public Breakable Tree { get; private set; }
    
    [Export] public Dictionary<int, Breakable> BreakablesDict { get; private set; }
    
    public Breakable GetById(int id) {
        return BreakablesDict[id];
    }
}