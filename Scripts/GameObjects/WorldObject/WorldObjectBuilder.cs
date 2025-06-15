using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class WorldObject {
    public static WorldObject FromDictionary(Dictionary dictionary) {
        IntVector coords = new(
            (int)dictionary["xPosition"].ToString().ToFloat(),
            (int)dictionary["yPosition"].ToString().ToFloat()
        );
        switch (dictionary["type"].ToString()) {
            case "block":
                Item item = Item.FromDictionary(dictionary["item"].AsGodotDictionary());
                return New(coords)
                    .AsBlock(item)
                    .Build();
            default:
                throw new ArgumentException($"Unknown WorldObject type: {dictionary["Type"]}");
        }
    }

    public static Builder New(IntVector coords) {
        return new Builder(coords);
    }

    public class Builder {
        private readonly WorldObject _worldObject;

        public Builder(IntVector coords) {
            _worldObject = new WorldObject();
            _worldObject.Coords = coords;
        }

        public Builder AsBlock(Item item) {
            _worldObject.ActiveProperties.AddRange(new List<ObjectProperty> {
                new ObjectCollision(_worldObject),
                new ObjectGatherable(_worldObject),
                new ObjectSpawnOnDeath(_worldObject, item),
                new ObjectHealth(_worldObject, 5),
                new ObjectTexture(_worldObject, item.IconTexture)
            });

            return this;
        }

        public Builder AsWall(Item item) {
            _worldObject.ActiveProperties.AddRange(new List<ObjectProperty> {
                new ObjectGatherable(_worldObject),
                new ObjectSpawnOnDeath(_worldObject, item),
                new ObjectHealth(_worldObject, 5),
                new ObjectTexture(_worldObject, item.IconTexture, true)
            });

            return this;
        }

        public Builder AsPickup(Item item) {
            _worldObject.ActiveProperties.AddRange(new List<ObjectProperty> {
                new ObjectTexture(_worldObject, item.IconTexture),
                new ObjectCanPickup(_worldObject, item),
            });

            return this;
        }

        public WorldObject Build() {
            _worldObject.ParentNode ??= new Node2D();

            foreach (ObjectProperty property in _worldObject.ActiveProperties) {
                property.Init();
            }

            _worldObject.AddChild(_worldObject.ParentNode, true);
            return _worldObject;
        }
    }
}