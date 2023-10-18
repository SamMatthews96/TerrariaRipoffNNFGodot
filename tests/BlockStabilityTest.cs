using System;
using TerrariaRipoffNNF.scripts;
using GdMUT;
using Godot;
using TerrariaRipoffNNF.scripts.BlockScripts;
using BlockResource = TerrariaRipoffNNF.scripts.BlockScripts.BlockResource;

namespace TerrariaRipoffNNF.tests;

public class ResolveExcessWeight {
    
    
    [CSTestFunction]
    public static Result ExcessWeightIsBlockWeightWhenOnTopOfStableBlock() {
        //ARRANGE
        BlockResource BlockResource = BlockTestHelpers.MakeBlockResource();
        BlockTestHelpers.MakeCellEmpty(0, 0);
        BlockTestHelpers.MakeCellEmpty(0, 1);

        //ACT
        Block bottomBlock = Block.CreateBlock(0, 0, BlockResource);
        Block topBlock = Block.CreateBlock(0, 1, BlockResource);

        //ASSERT
        return Math.Abs(BlockResource.Weight - topBlock.Stability.ExcessWeight) < 0.001f
            ? Result.Success
            : Result.Failure;
    }

    [CSTestFunction]
    public static Result ExcessWeightIsSameAsWeightWhenUnconnected() {
        //ARRANGE
        BlockResource blockResource = BlockTestHelpers.MakeBlockResource();
        for (int x = 0; x < 3; x++) {
            for (int y = 0; y < 3; y++) {
                BlockTestHelpers.MakeCellEmpty(x, y);
            }
        }

        //ACT
        Block block = Block.CreateBlock(0, 1, blockResource);

        //ASSERT
        return Math.Abs(block.BlockResource.Weight - block.Stability.ExcessWeight) < 0.001f
            ? Result.Success
            : Result.Failure;
    }

    [CSTestFunction]
    public static Result BlockOnlyAdjacentToLeftIsSupported() {
        //ARRANGE
        BlockResource blockResource = BlockTestHelpers.MakeBlockResource();

        for (int x = 0; x < 3; x++) {
            for (int y = 0; y < 3; y++) {
                BlockTestHelpers.MakeCellEmpty(x, y);
            }
        }

        //ACT
        Block bottom = Block.CreateBlock(0, 0, blockResource);
        Block middle = Block.CreateBlock(0, 1, blockResource);
        Block right = Block.CreateBlock(1, 1, blockResource);

        //ASSERT
        if (right.Stability.ExcessWeight != 0) return Result.Failure;
        if (Math.Abs(middle.Stability.ExcessWeight - 4) > 0.001f) return Result.Failure;
        return Result.Success;
    }

    [CSTestFunction]
    public static Result HeavyBlockHasExcess() {
        //ARRANGE
        BlockResource blockResource = new BlockResource {
            Name = "test",
            Weight = 10f,
            TensileStrength = 30f,
            MaxHealth = 50
        };

        for (int x = 0; x < 5; x++) {
            for (int y = 0; y < 5; y++) {
                BlockTestHelpers.MakeCellEmpty(x, y);
            }
        }

        //ACT
        Block bottom = Block.CreateBlock(0, 0, blockResource);
        Block middle = Block.CreateBlock(0, 1, blockResource);
        Block one = Block.CreateBlock(1, 1, blockResource);
        Block two = Block.CreateBlock(2, 1, blockResource);
        Block three = Block.CreateBlock(3, 1, blockResource);
        Block four = Block.CreateBlock(4, 1, blockResource);

        //ASSERT
        if (one.Stability.ExcessWeight != 0) return Result.Failure;
        if (two.Stability.ExcessWeight != 0) return Result.Failure;
        if (three.Stability.ExcessWeight != 0) return Result.Failure;
        if (Math.Abs(four.Stability.ExcessWeight - 10) > 0.001f) return Result.Failure;
        return Result.Success;
    }

    [CSTestFunction]
    public static Result SupportIsSplitWhenHeavy() {
        //ARRANGE
        BlockResource blockResource = new BlockResource {
            Name = "test",
            Weight = 10f,
            TensileStrength = 5f,
            MaxHealth = 50
        };

        for (int x = 0; x < 3; x++) {
            for (int y = 0; y < 3; y++) {
                BlockTestHelpers.MakeCellEmpty(x, y);
            }
        }

        //ACT
        Block bottomLeft = Block.CreateBlock(0, 0, blockResource);
        Block bottomRight = Block.CreateBlock(2, 0, blockResource);
        Block topLeft = Block.CreateBlock(0, 1, blockResource);
        Block topRight = Block.CreateBlock(2, 1, blockResource);
        Block topMiddle = Block.CreateBlock(1, 1, blockResource);

        //ASSERT
        if (topMiddle.Stability.ExcessWeight != 0) return Result.Failure;
        if (Math.Abs(topLeft.Stability.ExcessWeight - 15f) > 0.001f) return Result.Failure;
        if (Math.Abs(topRight.Stability.ExcessWeight - 15f) > 0.001f) return Result.Failure;
        return Result.Success;
    }

    [CSTestFunction]
    public static Result DeletingBlocksDeletesBlockSupports() {
        //ARRANGE
        BlockResource blockResource = new BlockResource {
            Name = "test",
            Weight = 10f,
            TensileStrength = 5f,
            MaxHealth = 50
        };

        for (int x = 0; x < 3; x++) {
            for (int y = 0; y < 3; y++) {
                BlockTestHelpers.MakeCellEmpty(x, y);
            }
        }

        Block bottom = Block.CreateBlock(0, 0, blockResource);
        Block middle = Block.CreateBlock(0, 1, blockResource);
        Block one = Block.CreateBlock(1, 1, blockResource);
        Block two = Block.CreateBlock(2, 1, blockResource);

        //ACT
        one.Destroy();

        //ASSERT
        return Math.Abs(blockResource.Weight - two.Stability.ExcessWeight) < 0.001f ? Result.Success : Result.Failure;
    }

    [CSTestFunction]
    public static Result DeletingBlocksRedirectsBlockSupports() {
        //ARRANGE
        BlockResource blockResource = BlockTestHelpers.MakeBlockResource();

        for (int x = 0; x < 5; x++) {
            for (int y = 0; y < 3; y++) {
                BlockTestHelpers.MakeCellEmpty(x, y);
            }
        }

        Block bottomLeft = Block.CreateBlock(0, 0, blockResource);
        Block left = Block.CreateBlock(0, 1, blockResource);
        Block middle = Block.CreateBlock(1, 1, blockResource);

        Block bottomRight = Block.CreateBlock(2, 0, blockResource);
        Block leftRight = Block.CreateBlock(2, 1, blockResource);

        //ACT
        left.Destroy();

        //ASSERT
        return middle.Stability.ExcessWeight == 0 ? Result.Success : Result.Failure;
    }

    [CSTestFunction]
    public static Result DeletingSupportRedirectsDependants() {
        //ARRANGE
        BlockResource BlockResource = BlockTestHelpers.MakeBlockResource();

        for (int x = 0; x < 5; x++) {
            for (int y = 0; y < 3; y++) {
                BlockTestHelpers.MakeCellEmpty(x, y);
            }
        }

        Block.CreateBlock(0, 0, BlockResource);
        Block.CreateBlock(0, 1, BlockResource);
        Block.CreateBlock(0, 2, BlockResource);

        Block.CreateBlock(1, 0, BlockResource);
        Block middle = Block.CreateBlock(1, 1, BlockResource);
        Block another = Block.CreateBlock(1, 2, BlockResource);

        Block farOut = Block.CreateBlock(2, 2, BlockResource);

        //ACT
        middle.Destroy();

        //ASSERT
        if (farOut.Stability.ExcessWeight != 0) return Result.Failure;
        if (another.Stability.ExcessWeight != 0) return Result.Failure;

        return Result.Success;
    }

    [CSTestFunction]
    public static Result AddingSupportBlockMakesAboveBlocksSupport() {
        //ARRANGE
        BlockResource BlockResource = BlockTestHelpers.MakeBlockResource();

        for (int x = 0; x < 5; x++) {
            for (int y = 0; y < 5; y++) {
                BlockTestHelpers.MakeCellEmpty(x, y);
            }
        }

        Block.CreateBlock(0, 0, BlockResource);
        Block.CreateBlock(0, 1, BlockResource);
        Block.CreateBlock(0, 2, BlockResource);

        Block.CreateBlock(1, 0, BlockResource);
        Block top = Block.CreateBlock(1, 2, BlockResource);


        //ACT
        Block block = Block.CreateBlock(1, 1, BlockResource);


        //ASSERT
        return top.Stability.IsSupportBlock ? Result.Success : Result.Failure;
    }

    [CSTestFunction]
    public static Result SupportColumnsSupportSumOfStructuresWeight() {
        //ARRANGE
        BlockResource BlockResource = BlockTestHelpers.MakeBlockResource();

        for (int x = 0; x < 5; x++) {
            for (int y = 0; y < 5; y++) {
                BlockTestHelpers.MakeCellEmpty(x, y);
            }
        }
        

        //ACT
        Block.CreateBlock(0, 0, BlockResource);
        var one = Block.CreateBlock(0, 1, BlockResource);
        var two = Block.CreateBlock(0, 2, BlockResource);
        var three = Block.CreateBlock(0, 3, BlockResource);

        Block.CreateBlock(1, 1, BlockResource);
        Block.CreateBlock(1, 2, BlockResource);
        Block.CreateBlock(1, 3, BlockResource);
        Block.CreateBlock(2, 1, BlockResource);
        Block.CreateBlock(2, 2, BlockResource);
        Block.CreateBlock(2, 3, BlockResource);


        //ASSERT
        float totalWeight = one.Stability.ExcessWeight + two.Stability.ExcessWeight
                                                       + three.Stability.ExcessWeight;
        return Math.Abs(totalWeight - 18f) < 0.001f ? Result.Success : Result.Failure;
    }

    [CSTestFunction]
    public static Result HeavyBlockIsUnstable() {
        //ARRANGE
        BlockResource blockResource = new BlockResource {
            Name = "test",
            Weight = 70f,
            TensileStrength = 30f,
            MaxHealth = 50
        };

        for (int x = 0; x < 5; x++) {
            for (int y = 0; y < 5; y++) {
                BlockTestHelpers.MakeCellEmpty(x, y);
            }
        }

        Block.CreateBlock(0, 0, blockResource);
        Block.CreateBlock(0, 1, blockResource);
        Block.CreateBlock(2, 0, blockResource);
        Block.CreateBlock(2, 1, blockResource);
        Block top = Block.CreateBlock(1, 2, blockResource);

        //ACT

        //ASSERT
        return top.Stability.IsStable ? Result.Failure : Result.Success;
    }

    [CSTestFunction]
    public static Result AddingBlockBelowUnstableBlockMakesItStable() {
        //ARRANGE
        BlockResource blockResource = new BlockResource {
            Name = "test",
            Weight = 70f,
            TensileStrength = 30f,
            MaxHealth = 50
        };

        for (int x = 0; x < 5; x++) {
            for (int y = 0; y < 5; y++) {
                BlockTestHelpers.MakeCellEmpty(x, y);
            }
        }

        Block.CreateBlock(0, 0, blockResource);
        Block.CreateBlock(0, 1, blockResource);
        Block.CreateBlock(2, 0, blockResource);
        Block.CreateBlock(2, 1, blockResource);
        Block top = Block.CreateBlock(1, 1, blockResource);

        //ACT
        Block.CreateBlock(1, 0, blockResource);

        //ASSERT
        return top.Stability.IsStable ? Result.Success : Result.Failure;
    }

    [CSTestFunction]
    public static Result AddingBlockToUnstableStructureMakesItStable() {
        //ARRANGE
        BlockResource blockResource = new BlockResource {
            Name = "test",
            Weight = 10f,
            TensileStrength = 30f,
            MaxHealth = 50
        };

        for (int x = 0; x < 10; x++) {
            for (int y = 0; y < 10; y++) {
                BlockTestHelpers.MakeCellEmpty(x, y);
            }
        }

        for (int i = 0; i < 10; i++) {
            Block.CreateBlock(0, i, blockResource);
        }

        Block.CreateBlock(1, 5, blockResource);
        var one = Block.CreateBlock(2, 5, blockResource);
        var two = Block.CreateBlock(2, 4, blockResource);
        var three = Block.CreateBlock(2, 3, blockResource);
        var four = Block.CreateBlock(2, 2, blockResource);

        //ACT
        var newBlock = Block.CreateBlock(1, 2, blockResource);
        

        //ASSERT
        if (!one.Stability.IsStable ||
            !two.Stability.IsStable ||
            !three.Stability.IsStable ||
            !four.Stability.IsStable ||
            !newBlock.Stability.IsStable
           ) {
            return new Result(false,
                $"{one.Stability.IsStable},{two.Stability.IsStable},{three.Stability.IsStable},{four.Stability.IsStable},{newBlock.Stability.IsStable},");
        }

        return Result.Success;
    }
}

public class ResolveUnstableGroups {
    [CSTestFunction]
    public static Result SingleSupportHasExcessBurdenEqualToExcessWeight() {
        //ARRANGE
        BlockResource blockResource = new BlockResource {
            Name = "test",
            Weight = 10f,
            TensileStrength = 20f,
            MaxHealth = 50
        };
        BlockTestHelpers.MakeBeginningEmpty(10);

        //ACT
        Block.CreateBlock(0, 0, blockResource);
        Block.CreateBlock(0, 1, blockResource);
        
        Block loadBearingBlock = Block.CreateBlock(1, 1, blockResource);
        Block.CreateBlock(1, 2, blockResource);
        Block.CreateBlock(1, 3, blockResource);
        Block.CreateBlock(1, 4, blockResource);
        //TotalWeight = 5*10 = 40f
        //TensileStrength = 20
        // expected: 40 - 20 / 20 = 1f

        //ASSERT
        if (Math.Abs(loadBearingBlock.Stability.ExcessBurden - 1) < 0.0001f) {
            return Result.Success;
        }

        return new Result(false, $"expected 1, received {loadBearingBlock.Stability.ExcessBurden}");
    }
    
    [CSTestFunction]
    public static Result UnsupportedBlockIsDestroyed() {
        //ARRANGE
        BlockResource blockResource = new BlockResource {
            Name = "test",
            Weight = 10f,
            TensileStrength = 20f,
            MaxHealth = 50
        };
        BlockTestHelpers.MakeBeginningEmpty(10);

        //ACT
        
        Block block = Block.CreateBlock(1, 1, blockResource);
        

        //ASSERT
        if (block.Stability.ExcessBurden >= 1000) {
            return Result.Success;
        }

        return new Result(false, $"expected 1000, received {block.Stability.ExcessBurden}");
    }
    
    [CSTestFunction]
    public static Result TwoSupportsShareBurden() {
        //ARRANGE
        BlockResource blockResource = new BlockResource {
            Name = "test",
            Weight = 10f,
            TensileStrength = 20f,
            MaxHealth = 50
        };
        BlockTestHelpers.MakeBeginningEmpty(10);

        //ACT
        
        Block.CreateBlock(0, 0, blockResource);
        Block.CreateBlock(0, 1, blockResource);
        Block.CreateBlock(3, 0, blockResource);
        Block.CreateBlock(3, 1, blockResource);
        
        Block supportOne = Block.CreateBlock(1, 1, blockResource);
        Block supportTwo = Block.CreateBlock(2, 1, blockResource);
        Block.CreateBlock(1, 2, blockResource);
        Block.CreateBlock(2, 2, blockResource);
        Block.CreateBlock(2, 3, blockResource);
        //TotalWeight = 5*10 = 50f
        //TensileStrength = 40
        // expected: 50 - 40 / 40 = 0.25f
        

        //ASSERT
        if (Math.Abs(supportOne.Stability.ExcessBurden - 0.25f) > 0.0001f) {
            return new Result(false, $"expected 0.25, received {supportOne.Stability.ExcessBurden}");
        }
        if (Math.Abs(supportTwo.Stability.ExcessBurden - 0.25f) > 0.0001f) {
            return new Result(false, $"expected 1000, received {supportTwo.Stability.ExcessBurden}");
        }
        
        return Result.Success;
        
    }
    
}