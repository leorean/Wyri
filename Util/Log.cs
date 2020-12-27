using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

public static class Logger
{
    public static void Log(object o)
    {
        Debug.WriteLine(o);
    }
}
