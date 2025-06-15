using Godot;

namespace TerrariaRipoffNNF;

public partial class WorldObject {
    public static Builder New(SavedObject savedObject, IntVector coords) {
        return new Builder(savedObject, coords);
    }
    public class Builder {
        private readonly WorldObject _worldObject;
        
        public Builder(SavedObject savedObject, IntVector coords) {
            _worldObject = new WorldObject();
            _worldObject.SavedObject = savedObject;
            _worldObject.Coords = coords;
        }

        public WorldObject Build() {
            foreach (ObjectProperty property in _worldObject.SavedObject.Properties) {
                property.Register(_worldObject);
            }

            _worldObject.ParentNode ??= new Node2D();

            foreach (ActiveObjectProperty property in _worldObject.ActiveProperties) {
                property.Init();
            }
            
            _worldObject.AddChild(_worldObject.ParentNode, true);
            return _worldObject;
        }
    }
}