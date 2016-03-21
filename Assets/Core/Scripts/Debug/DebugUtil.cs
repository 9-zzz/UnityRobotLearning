// USED IN CRUNCH
// DO NOT MOVE THIS FILE

using System;
using System.Diagnostics;
using System.Collections;

public static class DebugUtil
{
    public static void Assert(bool condition)
    {
        if (condition == false)
            throw new Exception("Assert failed");
    }

    public static void Assert(bool condition, object output)
    {
        if (condition == false)
            throw new Exception("Assert failed: " + output.ToString());
    }
}
