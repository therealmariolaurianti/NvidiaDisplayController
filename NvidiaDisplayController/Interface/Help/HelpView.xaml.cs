using System.Diagnostics;
using System.Windows.Navigation;

namespace NvidiaDisplayController.Interface.Help;

public partial class HelpView
{
    public HelpView()
    {
        InitializeComponent();
    }

    private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
        e.Handled = true;
    }
}