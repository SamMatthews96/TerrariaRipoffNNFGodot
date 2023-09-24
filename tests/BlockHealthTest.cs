using System;
using GdMUT;
using TerrariaRipoffNNF.scripts;


namespace TerrariaRipoffNNF.tests;


public class Health {
    [CSTestFunction]
    public static Result CreatedBlockHasHealthEqualToBlockSo() {
        //ARRANGE
        BlockResource blockSo = BlockTestHelpers.MakeBlockResource();
        BlockTestHelpers.MakeCellEmpty(0, 0);

        //ACT
        Block block = Block.CreateBlock(0, 0, blockSo);

        //ASSERT
        return Math.Abs(blockSo.MaxHealth - block.Health.CurrentHealth) < 0.001f ? Result.Success : Result.Failure;
    }


    [CSTestFunction]
    public static Result DamagingABlockReducesItsHealth() {
        //ARRANGE
        BlockResource blockSo = BlockTestHelpers.MakeBlockResource();
        BlockTestHelpers.MakeCellEmpty(0, 0);
        Block block = Block.CreateBlock(0, 0, blockSo);

        //ACT
        block.TakeDamage(30);

        //ASSERT
        float expected = blockSo.MaxHealth - 30;
        return Math.Abs(expected - block.Health.CurrentHealth) < 0.001f 
            ? Result.Success : Result.Failure;
    }

    [CSTestFunction]
    public static Result ReducingABlocksHealthToZeroDestroysIt() {
        //ARRANGE
        BlockResource blockSo = BlockTestHelpers.MakeBlockResource();
        BlockTestHelpers.MakeCellEmpty(0, 0);
        Block block = Block.CreateBlock(0, 0, blockSo);

        //ACT
        block.TakeDamage(60);

        //ASSERT
        var result = Block.GetBlockAtPosition(0, 0);
        return result is null ? Result.Success : Result.Failure;
    }
}