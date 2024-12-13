using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class Ingredient : ItemProperty {
    public override Dictionary ToDictionary() {
        throw new System.NotImplementedException();
    }

    [Export] private Array<Block> _testTupleArray = new();

    // public Array<> 
    // ingredient: type (stone, wood, metal)
    // quality: float

    /* examples
        Birch
            type: wood
            quality: 0.5
        Yew
            type: wood
            quality: 0.7
        Iron
            type: metal (strong)
            quality: 0.8
        Copper
            type: metal (conductive)
            quality: 0.6
        Silver
            type: metal (precious)
            quality: 0.7
        Gold
            type: metal (precious)
            quality: 0.8
        Diamond
            type: gem
            quality: 0.9
        Shadow Orb
            type: element
            quality: 0.8
            type: shadow
            quality: 0.8
        Light Orb
            type: element
            quality: 0.8
            type: light
            quality: 0.8
        Daybloom
            type: plant
            quality: 0.6
            type: herb(healing)
            quality: 0.6
        Deathweed
            type: plant
            quality: 0.6
            type: herb(poison)
            quality: 0.6
     */
}