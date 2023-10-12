#if TOOLS

using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace GdMUT.Components;

[Tool]
public partial class Dock : Control {
	[Export] private Button _runTests;

	[Export] private Button _loadTests;

	[Export] private VBoxContainer _testList;

	private List<TestFunction> _tests = new();
	private readonly Dictionary<Type, List<TestFunction>> _testDictionary = new();
	private readonly Dictionary<Type, TestClass> _testResultDictionary = new();

	public override void _EnterTree() {
		base._EnterTree();
		_runTests.Pressed += RunTests;
		_loadTests.Pressed += LoadTests;
	}

	private void LoadTests() {
		var stopwatch = new Stopwatch();
		stopwatch.Start();
		foreach (Node node in _testList.GetChildren()) {
			node.QueueFree();
		}

		_tests?.Clear();
		_tests = TestLoader.SearchForAllTests();
		_testDictionary.Clear();
		for (int testIndex = 0; testIndex < _tests.Count; testIndex++) {
			TestFunction function = _tests[testIndex];

			if (_testDictionary.TryGetValue(function.Type, out List<TestFunction> testList)) {
				testList.Add(function);
			} else {
				_testDictionary.Add(function.Type, new List<TestFunction>() { function });
			}
		}

		_testResultDictionary.Clear();
		var testResultScene = GD.Load<PackedScene>("res://addons/GDMUT/TestClass.tscn");
		foreach (Type type in _testDictionary.Keys) {
			var functions = _testDictionary[type];
			var testResult = testResultScene.Instantiate<TestClass>();
			testResult.SetTypeName(type.Name);
			_testList.AddChild(testResult);
			_testResultDictionary.Add(type, testResult);
			foreach (TestFunction function in functions) {
				testResult.AddMethodResult(function);
			}
		}

		stopwatch.Stop();
		GD.Print($"Loading tests took {stopwatch.ElapsedMilliseconds}ms");
	}

	private void RunTestsInRange(int startIndex, int endIndex) {
		for (int testIndex = startIndex; testIndex < endIndex; testIndex++) {
			var test = _tests[testIndex];
			GD.Print(test.Name);
			Result testResult;
			try {
				testResult = (Result)test.Method.Invoke(null, null);
			}
			catch (Exception e) {
				testResult = new Result(false, $"Exception thrown: {e.Message}");
			}

			test.Result = testResult;
		}
	}

	private void RunTests() {
		if (_tests.Count == 0) {
			GD.Print("No tests loaded");
			return;
		}

		var stopwatch = new Stopwatch();
		stopwatch.Start();

		GD.Print("Run Tests");
		RunTestsInRange(0, _tests.Count);

		stopwatch.Stop();
		UpdateUIWithResults();
		GD.Print($"Tests took {stopwatch.ElapsedMilliseconds}ms");
	}

	private void UpdateUIWithResults() {
		foreach (TestClass result in _testResultDictionary.Values) {
			result.UpdateResult();
		}
	}
}
#endif
