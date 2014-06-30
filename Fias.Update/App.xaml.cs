using System.Windows;
using System.Windows.Navigation;

namespace Fias.Update
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_OnLoadCompleted(object sender, NavigationEventArgs e)
        {
            DevExpress.Xpf.Core.ThemeManager.ApplicationThemeName = "Seven";
        }
    }
}
