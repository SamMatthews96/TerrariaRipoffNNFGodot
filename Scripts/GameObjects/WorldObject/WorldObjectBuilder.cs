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
        Item item = Item.FromDictionary(dictionary["item"].AsGodotDictionary());
        return dictionary["type"].ToString() switch {
            "block" => New(coords).AsBlock(item).Build(),
            "tree" => New(coords).AsTree(item).Build(),
            "pickup" => New(coords).AsPickup(item).Build(),
            _ => throw new ArgumentException($"Unknown WorldObject type: {dictionary["Type"]}")
        };
    }

    public Dictionary ToDictionary() {
        Dictionary dictionary = new() {
            { "type", Type },
            { "xPosition", Coords.X },
            { "yPosition", Coords.Y }
        };
        dictionary["item"] = Type switch {
            "block" or "tree" or "prop"
                => GetProperty<ObjectSpawnOnDeath>().Item.ToDictionary(),
            "pickup" => GetProperty<ObjectCanPickup>().Item.ToDictionary(),
            _ => throw new ArgumentException($"Unknown WorldObject type: {Type}")
        };

        return dictionary;
    }

    public static Builder New(IntVector coords) {
        return new Builder(coords);
    }

    public class Builder {
        private readonly WorldObject _worldObject;

        public Builder(IntVector coords) {
            _worldObject = Data.PackedScenes.WorldObject
                .Instantiate<WorldObject>();
            _worldObject.Coords = coords;
        }

        public Builder AsBlock(Item item) {
            _worldObject.Type = "block";
            _worldObject.ActiveProperties.AddRange(new List<ObjectProperty> {
                new ObjectCollision(_worldObject),
                new ObjectGatherable(_worldObject),
                new ObjectSpawnOnDeath(_worldObject, item),
                new ObjectHealth(_worldObject, 5),
                new ObjectTexture(_worldObject, item.IconTexture),
                new ObjectPlacementCollision(
                    _worldObject, PlacementCollisionLayer.Foreground)
            });

            return this;
        }

        public Builder AsWall(Item item) {
            _worldObject.Type = "wall";
            _worldObject.ActiveProperties.AddRange(new List<ObjectProperty> {
                new ObjectGatherable(_worldObject),
                new ObjectSpawnOnDeath(_worldObject, item),
                new ObjectHealth(_worldObject, 5),
                new ObjectTexture(_worldObject, item.IconTexture, true),
                new ObjectPlacementCollision(
                    _worldObject, PlacementCollisionLayer.Background)
            });

            return this;
        }

        public Builder AsTree(Item item) {
            _worldObject.Type = "tree";
            _worldObject.ActiveProperties.AddRange(new List<ObjectProperty> {
                new ObjectGatherable(_worldObject),
                new ObjectSpawnOnDeath(_worldObject, item),
                new ObjectHealth(_worldObject, 5),
                new ObjectTexture(_worldObject, item.IconTexture, true),
                new ObjectPlacementCollision(
                    _worldObject, PlacementCollisionLayer.Foreground)
            });

            return this;
        }

        public Builder AsProp(Item item) {
            _worldObject.Type = "prop";
            _worldObject.ActiveProperties.AddRange(new List<ObjectProperty> {
                new ObjectGatherable(_worldObject),
                new ObjectSpawnOnDeath(_worldObject, item),
                new ObjectHealth(_worldObject, 5),
                new ObjectTexture(_worldObject, item.IconTexture),
                new ObjectPlacementCollision(
                    _worldObject, PlacementCollisionLayer.Foreground)
            });
            if (item.TryGetProperty(out ItemCraftStation craftStation)) {
                _worldObject.ActiveProperties.Add(
                    new ObjectCraftStation(_worldObject, craftStation)
                );
            }


            return this;
        }

        public Builder AsComponent(WorldObject main) {
            _worldObject.Type = "component";
            _worldObject.ActiveProperties.AddRange(new List<ObjectProperty> {
                new ObjectComponent(_worldObject, main),
                new ObjectGatherable(_worldObject),
                new ObjectPlacementCollision(
                    _worldObject, PlacementCollisionLayer.Foreground)
            });
            return this;
        }

        public Builder AsPickup(Item item) {
            _worldObject.Type = "pickup";
            _worldObject.ActiveProperties.AddRange(new List<ObjectProperty> {
                new ObjectTexture(_worldObject, item.IconTexture),
                new ObjectCanPickup(_worldObject, item),
            });

            return this;
        }

        public WorldObject Build() {
            _worldObject.ParentNode ??= Data.PackedScenes.WorldStatic
                .Instantiate<Node2D>();

            foreach (ObjectProperty property in _worldObject.ActiveProperties) {
                property.Init();
            }

            _worldObject.AddChild(_worldObject.ParentNode, true);
            _worldObject.ParentNode.Position =
                (_worldObject.Coords * Game.BlockSize).ToVector2();
            return _worldObject;
        }
    }
}