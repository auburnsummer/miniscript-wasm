﻿using System;
using Miniscript;
using System.IO;
using System.Collections.Generic;

class MainClass {

	static void Print(string s) {
		Console.WriteLine(s);
	}

	static void ListErrors(Script script) {
		if (script.errors == null) {
			Print("No errors.");
			return;
		}
		foreach (Error err in script.errors) {
			Print(string.Format("{0} on line {1}: {2}",
				err.type, err.lineNum, err.description));
		}

	}

	static void Test(List<string> sourceLines, int sourceLineNum,
					 List<string> expectedOutput, int outputLineNum) {
		if (expectedOutput == null) expectedOutput = new List<string>();
//		Console.WriteLine("TEST (LINE {0}):", sourceLineNum);
//		Console.WriteLine(string.Join("\n", sourceLines));
//		Console.WriteLine("EXPECTING (LINE {0}):", outputLineNum);
//		Console.WriteLine(string.Join("\n", expectedOutput));

		Interpreter miniscript = new Interpreter(sourceLines);
		List<string> actualOutput = new List<string>();
		miniscript.standardOutput = (string s) => actualOutput.Add(s);
		miniscript.errorOutput = miniscript.standardOutput;
		miniscript.implicitOutput = miniscript.standardOutput;
		miniscript.RunUntilDone(60, false);

//		Console.WriteLine("ACTUAL OUTPUT:");
//		Console.WriteLine(string.Join("\n", actualOutput));

		int minLen = expectedOutput.Count < actualOutput.Count ? expectedOutput.Count : actualOutput.Count;
		for (int i = 0; i < minLen; i++) {
			if (actualOutput[i] != expectedOutput[i]) {
				Print(string.Format("TEST FAILED AT LINE {0}\n  EXPECTED: {1}\n    ACTUAL: {2}",
					outputLineNum + i, expectedOutput[i], actualOutput[i]));
			}
		}
		if (expectedOutput.Count > actualOutput.Count) {
			Print(string.Format("TEST FAILED: MISSING OUTPUT AT LINE {0}", outputLineNum + actualOutput.Count));
			for (int i = actualOutput.Count; i < expectedOutput.Count; i++) {
				Print("  MISSING: " + expectedOutput[i]);
			}
		} else if (actualOutput.Count > expectedOutput.Count) {
			Print(string.Format("TEST FAILED: EXTRA OUTPUT AT LINE {0}", outputLineNum + expectedOutput.Count));
			for (int i = expectedOutput.Count; i < actualOutput.Count; i++) {
				Print("  EXTRA: " + actualOutput[i]);
			}
		}

	}

	static void RunTestSuite(string path) {
		StreamReader file = new StreamReader(path);
		if (file == null) {
			Print("Unable to read: " + path);
			return;
		}

		List<string> sourceLines = null;
		List<string> expectedOutput = null;
		int testLineNum = 0;
		int outputLineNum = 0;

		string line = file.ReadLine();
		int lineNum = 1;
		while (line != null) {
			if (line.StartsWith("====")) {
				if (sourceLines != null) Test(sourceLines, testLineNum, expectedOutput, outputLineNum);
				sourceLines = null;
				expectedOutput = null;
			} else if (line.StartsWith("----")) {
				expectedOutput = new List<string>();
				outputLineNum = lineNum + 1;
			} else if (expectedOutput != null) {
				expectedOutput.Add(line);
			} else {
				if (sourceLines == null) {
					sourceLines = new List<string>();
					testLineNum = lineNum;
				}
				sourceLines.Add(line);
			}

			line = file.ReadLine();
			lineNum++;
		}
		if (sourceLines != null) Test(sourceLines, testLineNum, expectedOutput, outputLineNum);
		Print("\nIntegration tests complete.\n");
	}

	static void RunFile(string path, bool dumpTAC=false) {
		StreamReader file = new StreamReader(path);
		if (file == null) {
			Print("Unable to read: " + path);
			return;
		}

		List<string> sourceLines = new List<string>();
		while (!file.EndOfStream) sourceLines.Add(file.ReadLine());

		Interpreter miniscript = new Interpreter(sourceLines);
		miniscript.standardOutput = (string s) => Print(s);
		miniscript.implicitOutput = miniscript.standardOutput;
		miniscript.Compile();

		if (dumpTAC) {
			miniscript.vm.DumpTopContext();
		}
		
		while (!miniscript.done) {
			miniscript.RunUntilDone();
		}

	}

	public static void Main(string[] args) {
		
		Miniscript.HostInfo.name = "WASM";
		
		Print("Welcome to MiniScript WASM.\n");
	}
}
