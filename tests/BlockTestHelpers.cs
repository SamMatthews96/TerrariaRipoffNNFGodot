using TerrariaRipoffNNF.scripts;

namespace TerrariaRipoffNNF.tests; 

internal static class BlockTestHelpers {
    public static void MakeCellEmpty(int xPosition, int yPosition) {
        Block bottom = Block.GetBlockAtPosition(xPosition, yPosition);
        if (bottom is not null) {
            bottom.Destroy();
        }
    }

    public static Block MakeCellHaveBlock(int xPosition, int yPosition) {
        BlockResource blockResource = MakeBlockResource();
            
        Block block = Block.GetBlockAtPosition(xPosition, yPosition);
        if (block is null) {
            block = Block.CreateBlock(xPosition, yPosition, blockResource);
        }

        return block;
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