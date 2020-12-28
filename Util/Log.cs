using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

public static class Logger
{
    public static void Log(params object[] objs)
    {
        string log = "";
        for(var i = 0; i < objs.Length - 1; i++)
        {
            log += objs[i] + ", ";
        }
        log += objs.Last();

        Debug.WriteLine(log);
    }
}
