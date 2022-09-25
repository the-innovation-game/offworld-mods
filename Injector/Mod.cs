using System;
using System.Collections.Generic;
using System.Reflection;
using Offworld.AppCore;
using UnityEngine;

namespace Injector
{
    public class InjectorMod : ModEntryPointAdapter
    {
        public override void Initialize()
        {
            var UserScriptManager = Type.GetType("UserScriptManager, Assembly-CSharp");
            var flags = BindingFlags.NonPublic | BindingFlags.Static;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            (UserScriptManager.GetField("invalidDllReferenceNames", flags).GetValue(null) as List<string>).Clear();
            (UserScriptManager.GetField("invalidAssemblies", flags).GetValue(null) as List<string>).Clear();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }
        static int textCount = 0;
        public static void DisplayText(string text)
        {
            GUI.Label(new Rect(Screen.width - 200, 25 * textCount++, 200, 50), text);
        }
        public override void OnGUI()
        {
            textCount = 0;
            DisplayText("Injection Successful");
        }
    }
}
