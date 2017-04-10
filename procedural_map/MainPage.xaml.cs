using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace procedural_map {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page {
        public MainPage() {
            this.InitializeComponent();

            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;
            Application.Current.DebugSettings.EnableFrameRateCounter = false;

            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
            Window.Current.CoreWindow.KeyUp += CoreWindow_KeyUp;
        }

        private void CoreWindow_KeyUp(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args) { }
        private void CoreWindow_KeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args) {
            switch (args.VirtualKey) {
                case Windows.System.VirtualKey.Right:
                case Windows.System.VirtualKey.Left:
                case Windows.System.VirtualKey.Up:
                case Windows.System.VirtualKey.Down:
                    Camera.KeyDown(args.VirtualKey);
                    break;
            }
        }

        private void canvasMain_Draw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args) {
            Stopwatch s = Stopwatch.StartNew();
            Map.Draw(args);
            Debug.Draw(args);
            s.Stop();

            Debug.LastDrawTime = s.ElapsedMilliseconds.ToString();
        }

        private void canvasMain_Update(ICanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args) {
            Stopwatch s = Stopwatch.StartNew();
            Map.Update(args);
            s.Stop();

            Debug.LastUpdateTime = s.ElapsedMilliseconds.ToString();
            Debug.Update(args);
        }

        private void canvasMain_CreateResources(CanvasAnimatedControl sender, CanvasCreateResourcesEventArgs args) {
            Map.Initialize(sender.Device);
        }

        private void canvasMain_PointerMoved(object sender, PointerRoutedEventArgs e) {

        }
    }
}
