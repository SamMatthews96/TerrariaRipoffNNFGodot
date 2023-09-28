#if TOOLS
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Godot;

namespace GdMUT;

public class TestFunction
{
    public string Name { get; set; }
    public Type Type { get; set; }
    public MethodInfo Method { get; set; }
    public Result Result { get; set; }

    public Func<Result> GetAction() {
        try {
            GD.Print(Method);
            if (Method is null) throw new Exception();
            return (Func<Result>)Delegate.CreateDelegate(typeof(Func<Result>), null, Method);
        }
        catch (Exception e) {
            GD.Print(e.Message);

            return () => {
                GD.Print($"couldn't convert {Name} to Action");
                return Result.Failure;
            };
        }
    }
}
#endif
