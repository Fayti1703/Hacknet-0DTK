﻿using Hacknet;
using Hacknet.Extensions;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ZeroDayToolKit.Patches
{
    public class ExtensionSequencerExeInstantActivate
    {
        public static List<ExtensionSequencerExe> queue = new List<ExtensionSequencerExe>();

        [HarmonyLib.HarmonyPatch]
        public class PatchConstructor
        {
            static MethodBase TargetMethod()
            {
                return typeof(ExtensionSequencerExe).GetConstructor(new Type[] { typeof(Rectangle), typeof(OS), typeof(string[]) });
            }

            static void Postfix(ExtensionSequencerExe __instance, Rectangle location, OS operatingSystem, string[] p)
            {
                if (p.Length > 1 && p[1].ToLower() == "-i") queue.Add(__instance);
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(ExtensionSequencerExe), nameof(ExtensionSequencerExe.LoadContent))]
        public class PatchLoadContent
        {
            static void Postfix(ExtensionSequencerExe __instance)
            {
                if (queue.Contains(__instance))
                {
                    queue.Remove(__instance);
                    if (__instance.os.TraceDangerSequence.IsActive)
                    {
                        __instance.os.write("SEQUENCER ERROR: OS reports critical action already in progress.");
                    }
                    else // go immediately
                    {
                        __instance.stateTimer = 0.0f;
                        __instance.state = ExtensionSequencerExe.SequencerExeState.SpinningUp;
                        __instance.bars.MinLineChangeTime = 0.1f;
                        __instance.bars.MaxLineChangeTime = 1f;
                        MusicManager.FADE_TIME = 0.6f;
                        __instance.oldSongName = MusicManager.currentSongName;
                        __instance.targetComp = Programs.getComputer(__instance.os, __instance.targetID);
                        ((WebServerDaemon)__instance.targetComp.getDaemon(typeof(WebServerDaemon)))?.LoadWebPage();
                        RunnableConditionalActions.LoadIntoOS(ExtensionLoader.ActiveExtensionInfo.ActionsToRunOnSequencerStart, __instance.os);
                    }
                }
            }
        }
    }
}
