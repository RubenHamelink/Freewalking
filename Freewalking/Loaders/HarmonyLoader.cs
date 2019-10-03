using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Freewalking.UI;
using Harmony;
using ICities;

namespace Freewalking.Loaders
{
    public class HarmonyLoader : ILoadingExtension
    {
        private HarmonyInstance harmony;

        public void OnCreated(ILoading loading)
        {
            harmony = HarmonyInstance.Create("com.company.project.product");
        }

        public void OnReleased()
        {
            harmony.UnpatchAll();
        }

        public void OnLevelLoaded(LoadMode mode)
        {
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public void OnLevelUnloading()
        {
            harmony.UnpatchAll();
        }
    }
    
    [HarmonyPatch(typeof(CinematicCameraController))]
    [HarmonyPatch("Update")]
    [HarmonyPatch(new Type[] { })]
    public class Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original,
            IEnumerable<CodeInstruction> instr,
            ILGenerator generator)
        {
            // The code from the actual method is to check if the cinematic camera should be aborted:
            // if (!Input.anyKey || this.m_ShortcutInGameShortcutCinematicCameraMode.IsPressed())
            // return;

            // This changes the code to delegate to our own FreewalkingCamera.ShouldAbort method
            Label end = generator.DefineLabel();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(CinematicCameraController), "m_CurrentScript"));
            generator.Emit(OpCodes.Call, AccessTools.Method(typeof(FreewalkingCamera), nameof(FreewalkingCamera.ShouldAbort), new []{typeof(ICameraExtension)}));
            generator.Emit(OpCodes.Ldc_I4_1);
            generator.Emit(OpCodes.Ceq);
            generator.Emit(OpCodes.Brfalse, end);

            // The normal abort code from the Update method
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Call, AccessTools.Method(typeof(CinematicCameraController), "AbortScript"));
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldc_I4_0);
            generator.Emit(OpCodes.Call,
                AccessTools.Method(typeof(CinematicCameraController),"set_enabled", new Type[] {typeof(bool)}));

            generator.MarkLabel(end);
            generator.Emit(OpCodes.Ret);
            return instr;
        }
    }
}