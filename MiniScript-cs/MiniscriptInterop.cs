using System;
using System.Collections.Generic;
using Microsoft.JSInterop;

/*
Interface from JS to MiniScript.

At the moment, this is entirely designed towards the VScode usecase of verifying and highlighting
compiled code.

(i.e. we're not going to do JS intrinsics for now)
*/
namespace Miniscript {
    public static class MiniscriptInterop {
        [JSInvokable]
        public static DotNetObjectReference<Interpreter> MakeInterpreter(string code) {
            Interpreter inte = new Interpreter();
            inte.Reset(code);
            return DotNetObjectReference.Create(inte);
        }

        [JSInvokable]
        public static void TryCompile(this Interpreter inte) {
            inte.Compile();
        }
    }
}