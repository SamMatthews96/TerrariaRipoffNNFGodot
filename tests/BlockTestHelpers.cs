using TerrariaRipoffNNF.scripts;

namespace TerrariaRipoffNNF.tests; 

internal static class BlockTestHelpers {
    public static void MakeCellEmpty(int xPosition, int yPosition) {
        Block.GetBlockAtPosition(xPosition, yPosition)?.Destroy();
    }

    public static Block MakeCellHaveBlock(int xPosition, int yPosition) {
        return Block.GetBlockAtPosition(xPosition, yPosition) 
               ?? Block.CreateBlock(xPosition, yPosition, MakeBlockResource());
    }

    public static BlockResource MakeBlockResource() {
        return new BlockResource {
            Name = "test",
            Weight = 2f,
            TensileStrength = 30f,
            MaxHealth = 50
        };
    }
}