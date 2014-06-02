using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Imb
{
    /// <summary>
    /// This class captures and sets window positions using a WPF window object
    /// </summary>
    public class WindowPositionSettings
    {
        private Window _window;

        public WindowPositionData PositionData { private set; get; }

        // ReSharper disable CompareOfFloatsByEqualityOperator
        public WindowPositionSettings(WindowPositionData positionData, double defaultScreenProportion)
        {
            if (positionData == null)
                CreateCentredPosition(defaultScreenProportion);
            else
                PositionData = positionData;

            if (PositionData.Width == 0 || PositionData.Height == 0)
                SetDefaultPosition(PositionData, defaultScreenProportion);
        }

        private void SetDefaultPosition(WindowPositionData positionData, double defaultScreenProportion)
        {
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;
            var desiredHeight = screenHeight * defaultScreenProportion;
            var desiredWidth = screenWidth * defaultScreenProportion;
            positionData.Height = desiredHeight;
            positionData.Width = desiredWidth;
            positionData.Top = (screenHeight - desiredHeight) / 2;
            positionData.Left = (screenWidth - desiredWidth) / 2;
        }


        private void CreateCentredPosition(double defaultScreenProportion)
        {
            PositionData = new WindowPositionData();
            SetDefaultPosition(PositionData, defaultScreenProportion);
        }

        public void AttachWindow(Window window)
        {
            _window = window;
            _window.SizeChanged += WindowSizeChanged;
            _window.LocationChanged += WindowLocationChanged;

            _window.Width = PositionData.Width;
            _window.Height = PositionData.Height;
            _window.Left = PositionData.Left;
            _window.Top = PositionData.Top;
        }

        private void WindowLocationChanged(object sender, EventArgs e)
        {
            PositionData.Left = _window.Left;
            PositionData.Top = _window.Top;
        }

        private void WindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            PositionData.Width = _window.Width;
            PositionData.Height = _window.Height;
        }

    }

    /// <summary>
    /// This class encapsulates a window's position and size.
    /// </summary>
    [Serializable]
    public class WindowPositionData
    {
        [SettingsSerializeAs(SettingsSerializeAs.Xml)]
        public double Left { get; set; }

        [SettingsSerializeAs(SettingsSerializeAs.Xml)]
        public double Top { get; set; }

        [SettingsSerializeAs(SettingsSerializeAs.Xml)]
        public double Width { get; set; }

        [SettingsSerializeAs(SettingsSerializeAs.Xml)]
        public double Height { get; set; }

        [SettingsSerializeAs(SettingsSerializeAs.Xml)]
        public string WindowId { get; set; }
    }
}
