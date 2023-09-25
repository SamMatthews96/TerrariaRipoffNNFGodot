#if TOOLS
using System;
using Godot;

namespace GdMUT.Components;

[Tool]
public partial class MethodResult : Control
{
    [Export]
    private RichTextLabel _methodName;

    [Export]
    private RichTextLabel _result;
    private TestFunction _function;

    [Export] private Button _runTest;

    public override void _EnterTree()
    {
        base._EnterTree();
        _runTest.Pressed += RunTest;
    }

    private void RunTest() {
        
        var test = _function;
        GD.Print(test.Name);
        Result testResult;
        try
        {
            testResult = (Result)test.Method.Invoke(null, null);
            GD.Print(testResult.IsSuccess);
        }
        catch (Exception e) {
            testResult = new Result(false, $"Exception thrown: {e.Message}");
            GD.Print(e.Message);
        }

        test.Result = testResult;
    }
    
    public void SetMethodResult(TestFunction function)
    {
        _function = function;
        _methodName.Text = function.Method.Name;
        Reset();
    }

    public void Update()
    {
        SetSuccess(_function.Result.IsSuccess, _function.Result.Message);
    }

    public void Reset()
    {
        _result.Text = "";
        SelfModulate = new Color(1, 1, 1);
    }

    public void SetSuccess(bool success, string result = "")
    {
        _result.Text = (success ? "Success: " : "Failure: ") + result;
        Modulate = success ? new Color(0, 1, 0) : new Color(1, 0, 0);
        GD.Print($"{result} {success}");
    }
    
}
#endif
