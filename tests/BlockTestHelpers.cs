using TerrariaRipoffNNF.scripts;

namespace TerrariaRipoffNNF.tests; 

internal static class BlockTestHelpers {
    public static void MakeCellEmpty(int xPosition, int yPosition) {
        Block block = Block.GetBlockAtPosition(xPosition, yPosition);
        block?.Destroy();
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

    public static void MakeBeginningEmpty(int square) {
        for (int x = 0; x < square; x++) {
            for (int y = 0; y < square; y++) {
                Block.GetBlockAtPosition(x, y)?.Destroy();
            }
        }
    }
}