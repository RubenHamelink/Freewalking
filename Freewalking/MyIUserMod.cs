using ICities;

namespace Freewalking
{

    public class MyIUserMod: IUserMod
    {

        public string Name 
        {
            get { return "Freewalking Mod"; }
        }

        public string Description 
        {
            get { return "Best mod for walking freely around your city!"; }
			
        }
    }
}
