using ColossalFramework.Plugins;
using ColossalFramework.UI;
using ICities;
using UnityEngine;

namespace Freewalking
{

    public class Loader: ILoadingExtension
    {
        // Thread: Main
		public void OnCreated(ILoading loading)
		{
		}
		// Thread: Main
		public void OnReleased()
		{
	
		}
		public void OnLevelLoaded(LoadMode mode)
        {
            UIView v = UIView.GetAView();
            v.AddUIComponent(typeof(SettingsPanel));
        }
		public void OnLevelUnloading()
		{
			
		}
		
	 }
}
