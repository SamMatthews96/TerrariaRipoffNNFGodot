using System;
using Godot;

namespace TerrariaRipoffNNF.testScripts;

public partial class Test : Node {
    // private 
    private int _test = 0;

    public override void _Ready() {
        TimeMethod(MyMethod);
        GD.Print(_test);
    }

    private void MyMethod() {
        _test += 10;
    }
    
    private void TimeMethod(Action action) {
        var watch = new System.Diagnostics.Stopwatch();
        watch.Start();
        for (int i = 0; i < 10000000; i++) {
            action();
        }
        watch.Stop();
        GD.Print(watch.ElapsedMilliseconds);
    }
    
    
}

// public delegate void Actshun();
