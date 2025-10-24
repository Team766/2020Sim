using UnityEngine;
using System;
using System.Collections.Specialized;
using System.Web;

public class ApplicationArguments
{
    public static string GetArgument(string argumentName)
    {
#if UNITY_EDITOR
        if (argumentName == "robotPublicKey")
        {
            return "thePublicKey";
        }
        if (argumentName == "robotPrivateKey")
        {
            return "itsSecret";
        }
        return null;
#elif UNITY_WEBGL
        var uri = new Uri(Application.absoluteURL);
        return HttpUtility.ParseQueryString(uri).Get(argumentName);
#else
        string search = "--" + argumentName + "=";
        foreach (var arg in Environment.GetCommandLineArgs())
        {
            if (arg.StartsWith(search))
            {
                return arg.Substring(search.Length);
            }
        }
        return null;
#endif
    }
}
