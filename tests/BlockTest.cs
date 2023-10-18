using System;
using System.Collections.Generic;
using GdMUT;
using Godot;
using TerrariaRipoffNNF.scripts;
using TerrariaRipoffNNF.scripts.BlockScripts;
using static Godot.GD;
using BlockResource = TerrariaRipoffNNF.scripts.BlockScripts.BlockResource;

namespace TerrariaRipoffNNF.tests;
#if TOOLS
public class CreateBlock {
    [CSTestFunction]
    public static Result CreatesABlock() {
        //ARRANGE
        BlockTestHelpers.MakeBeginningEmpty(3);
        BlockResource blockSo = BlockTestHelpers.MakeBlockResource();
        BlockTestHelpers.MakeCellEmpty(0, 0);

        //ACT
        Block block = Block.CreateBlock(0, 0, blockSo);

        //ASSERT
        return block is null ? Result.Failure : Result.Success;
    }
}

public class DestroyBlock {
    [CSTestFunction]
    public static Result DeletesBlockFromWorldBlocks() {
        try {
            //ARRANGE
            BlockTestHelpers.MakeBeginningEmpty(3);
            Block block = BlockTestHelpers.MakeCellHaveBlock(0, 0);


            //ACT
            block.Destroy();

            //ASSERT
            var blockAtPosition = Block.GetBlockAtPosition(0, 0);
            return blockAtPosition is null ? Result.Success : Result.Failure;
        }
        catch (Exception e) {
            return Result.Failure;
        }

    }
}

public class GetBlockBeneath {
    [CSTestFunction]
    public static Result ReturnsNullWhenAtBottom() {
        //ARRANGE
        BlockTestHelpers.MakeBeginningEmpty(3);
        Block block = BlockTestHelpers.MakeCellHaveBlock(0, 0);

        //ACT
        Block blockBeneath = block.GetBlockInDirection(Direction.Down);

        //ASSERT
        return blockBeneath is null ? Result.Success : Result.Failure;
    }

    [CSTestFunction]
    public static Result ReturnsNullWhenNothingIsBelow() {
        //ARRANGE
        BlockTestHelpers.MakeBeginningEmpty(5);
        Block top = BlockTestHelpers.MakeCellHaveBlock(0, 1);

        //ACT
        Block blockBeneath = top.GetBlockInDirection(Direction.Down);

        //ASSERT
        return blockBeneath is null ? Result.Success : Result.Failure;
    }

    [CSTestFunction]
    public static Result ReturnsBlockBelow() {
        //ARRANGE
        BlockTestHelpers.MakeBeginningEmpty(5);
        Block bottom = BlockTestHelpers.MakeCellHaveBlock(0, 0);
        Block top = BlockTestHelpers.MakeCellHaveBlock(0, 1);

        //ACT
        Block blockFoundBeneath = top.GetBlockInDirection(Direction.Down);

        //ASSERT
        return bottom == blockFoundBeneath ? Result.Success : Result.Failure;
    }
}

public class GetBlockAbove {
    [CSTestFunction]
    public static Result ReturnsNullWhenAtTop() {
        //ARRANGE
        Block block = BlockTestHelpers.MakeCellHaveBlock(0, Block.WORLD_HEIGHT - 1);

        //ACT
        Block blockAbove = block.GetBlockInDirection(Direction.Up);

        //ASSERT
        return blockAbove is null ? Result.Success : Result.Failure;
    }

    [CSTestFunction]
    public static Result ReturnsNullWhenNothingIsAbove() {
        //ARRANGE
        BlockTestHelpers.MakeCellEmpty(0, 1);
        Block bottom = BlockTestHelpers.MakeCellHaveBlock(0, 0);

        //ACT
        Block blockAbove = bottom.GetBlockInDirection(Direction.Up);

        //ASSERT
        return blockAbove is null ? Result.Success : Result.Failure;
    }

    [CSTestFunction]
    public static Result ReturnsBlockAbove() {
        //ARRANGE
        Block bottom = BlockTestHelpers.MakeCellHaveBlock(0, 0);
        Block top = BlockTestHelpers.MakeCellHaveBlock(0, 1);

        //ACT
        Block blockFound = bottom.GetBlockInDirection(Direction.Up);

        //ASSERT
        return top == blockFound ? Result.Success : Result.Failure;
    }
}

public class GetBlockToLeft {
    [CSTestFunction]
    public static Result ReturnsNullWhenOnLeftSide() {
        //ARRANGE
        BlockTestHelpers.MakeCellEmpty(0, 1);
        BlockTestHelpers.MakeCellEmpty(1, 0);
        BlockTestHelpers.MakeCellEmpty(1, 1);
        Block block = BlockTestHelpers.MakeCellHaveBlock(0, 0);

        //ACT
        Block blockFound = block.GetBlockInDirection(Direction.Left);

        //ASSERT
        return blockFound is null ? Result.Success : Result.Failure;
    }

    [CSTestFunction]
    public static Result ReturnsNullWhenNothingIsLeft() {
        //ARRANGE
        BlockTestHelpers.MakeCellEmpty(0, 0);
        Block right = BlockTestHelpers.MakeCellHaveBlock(1, 0);

        //ACT
        Block blockFound = right.GetBlockInDirection(Direction.Left);

        //ASSERT
        return blockFound is null ? Result.Success : Result.Failure;
    }

    [CSTestFunction]
    public static Result ReturnsBlockToLeft() {
        //ARRANGE
        Block left = BlockTestHelpers.MakeCellHaveBlock(0, 0);
        Block right = BlockTestHelpers.MakeCellHaveBlock(1, 0);

        //ACT
        Block blockFound = right.GetBlockInDirection(Direction.Left);

        //ASSERT
        return left == blockFound ? Result.Success : Result.Failure;
    }
}

public class GetBlockToRight {
    [CSTestFunction]
    public static Result ReturnsNullWhenOnRight() {
        //ARRANGE
        Block block = BlockTestHelpers.MakeCellHaveBlock(Block.WORLD_WIDTH - 1, 0);

        //ACT
        Block blockFound = block.GetBlockInDirection(Direction.Right);

        //ASSERT
        return blockFound is null ? Result.Success : Result.Failure;
    }

    [CSTestFunction]
    public static Result ReturnsNullWhenNothingIsRight() {
        //ARRANGE
        BlockTestHelpers.MakeCellEmpty(1, 0);
        Block left = BlockTestHelpers.MakeCellHaveBlock(0, 0);

        //ACT
        Block blockFound = left.GetBlockInDirection(Direction.Right);

        //ASSERT
        return blockFound is null ? Result.Success : Result.Failure;
    }

    [CSTestFunction]
    public static Result ReturnsBlockToRight() {
        //ARRANGE
        Block left = BlockTestHelpers.MakeCellHaveBlock(0, 0);
        Block right = BlockTestHelpers.MakeCellHaveBlock(1, 0);

        //ACT
        Block blockFound = left.GetBlockInDirection(Direction.Right);

        //ASSERT
        return right == blockFound ? Result.Success : Result.Failure;
    }
}

public class GetBlockAtPosition {
    [CSTestFunction]
    public static Result ReturnsNullIfCellEmpty() {
        //ARRANGE
        BlockTestHelpers.MakeCellEmpty(0, 0);

        //ACT
        Block blockFound = Block.GetBlockAtPosition(0, 0);

        //ASSERT
        return blockFound is null ? Result.Success : Result.Failure;
    }

    [CSTestFunction]
    public static Result ReturnsBlockIfCellNotEmpty() {
        //ARRANGE
        BlockTestHelpers.MakeCellHaveBlock(0, 0);

        //ACT
        Block blockFound = Block.GetBlockAtPosition(0, 0);

        //ASSERT
        return blockFound is not null ? Result.Success : Result.Failure;
    }
}

public class GetBlocksInArea {
    [CSTestFunction]
    public static Result ReturnsEmptyListIfNoBlocksInArea() {
        //ARRANGE
        BlockTestHelpers.MakeBeginningEmpty(10);


        //ACT
        List<Block> blocks = Block.GetBlocksInArea(0, 0, 4, 4);

        //ASSERT
        
        return blocks.Count == 0 ? Result.Success : Result.Failure;
    }

    [CSTestFunction]
    public static Result ReturnsListIfBlocksInArea() {
        //ARRANGE
        BlockTestHelpers.MakeBeginningEmpty(10);

        BlockTestHelpers.MakeCellHaveBlock(0, 0);
        BlockTestHelpers.MakeCellHaveBlock(0, 1);
        BlockTestHelpers.MakeCellHaveBlock(0, 2);

        //ACT
        List<Block> blocks = Block.GetBlocksInArea(0, 0, 4, 4);

        //ASSERT
        return blocks.Count == 3 ? Result.Success : Result.Failure;
    }
}

#endif