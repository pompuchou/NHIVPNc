using System;
using WindowsInput;
using WindowsInput.Native;

namespace NHIVPNc
{
    internal partial class VPN_Downloader : IDisposable
    {
        private void Work_todo()
        {
            log.Info("Enter Work_todo.");
            InputSimulator sim = new InputSimulator();
            log.Info("Press s");
            System.Threading.Thread.Sleep(4000);
            sim.Keyboard.KeyPress(VirtualKeyCode.VK_S);
            log.Info("  Press Enter.");
            System.Threading.Thread.Sleep(1000);
            sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
            log.Info("Exit Work_todo.");
        }
    }
}
