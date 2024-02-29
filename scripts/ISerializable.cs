using System;
using Godot.Collections;


namespace TerrariaRipoffNNF.scripts;

public interface ISerializable {
    public Dictionary Serialize();
}