using Fragsurf.Shared;
using Fragsurf.UI;
using System.Linq;

namespace Fragsurf.Client
{
    [Inject(InjectRealm.Client)]
    public class DynamicSettings : FSSharedScript
    {

        protected override void OnGameLoaded()
        {
            var settingsModal = UGuiManager.Instance.Find<Modal_Settings>();
            if (settingsModal)
            {
                settingsModal.CreatePage("server", DevConsole.GetVariablesWithFlags(ConVarFlags.Replicator).Distinct().ToList());
                settingsModal.CreatePage("gamemode", DevConsole.GetVariablesWithFlags(ConVarFlags.Gamemode).Distinct().ToList());
            }
        }

        protected override void _Destroy()
        {
            if (!UGuiManager.Instance)
            {
                return;
            }
            var settingsModal = UGuiManager.Instance.Find<Modal_Settings>();
            if (settingsModal)
            {
                settingsModal.RemovePage("server");
                settingsModal.RemovePage("gamemode");
            }
        }

    }
}

