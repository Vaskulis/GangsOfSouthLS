﻿using Rage;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace GangsOfSouthLS.INIFile
{
    internal static class INIReader
    {
        private static InitializationFile iniFile;
        internal static Keys MenuKey;
        internal static Keys MenuModifierKey;
        internal static string UnitName;

        internal static bool RequestAirSupport;

        internal static List<string> UnmarkedCarList;

        internal static bool LoadINIFile()
        {
            iniFile = new InitializationFile(@"Plugins/LSPDFR/GangsOfSouthLS.ini");
            if (!iniFile.Exists())
            {
                Game.LogTrivial("[GangsOfSouthLS] Could not find INI File. Make sure you installed GangsOfSouthLS correctly. Not loading GangsOfSouthLS...");
                Game.DisplaySubtitle("GangsOfSouthLS could not find its INI File. Make sure you installed it correctly. Not loading GangsOfSouthLS...");
                return false;
            }
            SetMenuKey();
            SetMenuModifierKey();
            SetUnitName();
            SetDrugDealOptions();
            SetProtectionRacketeeringOptions();
            return true;
        }

        private static void SetMenuKey()
        {
            var keyString = iniFile.ReadString("KEYBINDS", "MenuKey");
            if (Enum.TryParse(keyString, false, out MenuKey))
            {
                Game.LogTrivial(string.Format("Set MenuKey to {0}", keyString));
            }
            else
            {
                Game.LogTrivial("[GangsOfSouthLS] Couldn't read player specified value for MenuKey, using default value L");
                Game.DisplayNotification("~b~GangsOfSouthLS: ~r~Couldn't read player specified value for MenuKey, using default value L!");
                MenuKey = Keys.L;
            }
        }

        private static void SetMenuModifierKey()
        {
            var keyString = iniFile.ReadString("KEYBINDS", "MenuModifierKey");
            if (Enum.TryParse(keyString, false, out MenuModifierKey))
            {
                Game.LogTrivial(string.Format("Set MenuKey to {0}", keyString));
            }
            else
            {
                Game.LogTrivial("[GangsOfSouthLS] Couldn't read player specified value for MenuModifierKey, using default value LControlKey");
                Game.DisplayNotification("~b~GangsOfSouthLS: ~r~Couldn't read player specified value for MenuModifierKey, using default value LControlKey!");
                MenuModifierKey = Keys.LControlKey;
            }
        }

        private static void SetUnitName()
        {
            var DivisionString = iniFile.ReadString("PERSONAL", "Division");
            var UnitTypeString = iniFile.ReadString("PERSONAL", "UnitType");
            var BeatString = iniFile.ReadString("PERSONAL", "Beat");
            UnitName = "";

            if (DivisionString.Length == 1)
            {
                UnitName += ("DIV_0" + DivisionString + " ");
            }
            else
            {
                UnitName += ("DIV_" + DivisionString + " ");
            }

            UnitName += (UnitTypeString + " ");

            if (BeatString.Length == 1)
            {
                UnitName += ("BEAT_0" + BeatString);
            }
            else
            {
                UnitName += ("BEAT_" + BeatString);
            }
        }

        private static void SetDrugDealOptions()
        {
            RequestAirSupport = iniFile.ReadBoolean("DRUGDEAL-CALLOUT", "RequestAirSupport", true);
        }

        private static void SetProtectionRacketeeringOptions()
        {
            var UnmarkedCarListString = iniFile.ReadString("[PROTECTIONRACKETEERING-CALLOUT]", "UnmarkedCarList", "POLICE4, FBI, FBI2");
            UnmarkedCarList = new List<string> { };
            foreach (var car in UnmarkedCarListString.Replace(" ", string.Empty).Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries))
            {
                UnmarkedCarList.Add(car.ToLower());
            }
        }
    }
}