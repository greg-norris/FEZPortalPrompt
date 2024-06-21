using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using GHIElectronics.TinyCLR.Devices.Display;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Pins;
using System.Drawing;
using GHIElectronics.TinyCLR.IO;
using GHIElectronics.TinyCLR.Devices.Storage;

namespace FEZPortalPrompt{
    class Program{
        static string prompt = "";
        static int speed = 10; 
        static void Main(){

            var sd = StorageController.FromName(SC20260.StorageController.SdCard);
            var drive = FileSystem.Mount(sd.Hdc);

            var directory = new DirectoryInfo(drive.Name);
            var files = directory.GetFiles();

            foreach (var f in files){
                Debug.WriteLine(f.Name);
                Debug.WriteLine(f.Length.ToString());
                byte[] textToPrompt = System.IO.File.ReadAllBytes(f.Name);

                prompt = System.Text.Encoding.UTF8.GetString(textToPrompt);

                Debug.WriteLine(prompt);
            }

            GpioPin backlight = GpioController.GetDefault().OpenPin(SC20260.GpioPin.PA15);
            backlight.SetDriveMode(GpioPinDriveMode.Output);
            backlight.Write(GpioPinValue.High);

            var displayController = DisplayController.GetDefault();

            displayController.SetConfiguration(new ParallelDisplayControllerSettings{
                Width = 480,
                Height = 272,
                DataFormat = DisplayDataFormat.Rgb565,
                Orientation = DisplayOrientation.Degrees0, //Rotate display.
                PixelClockRate = 10000000,
                PixelPolarity = false,
                DataEnablePolarity = false,
                DataEnableIsFixed = false,
                HorizontalFrontPorch = 2,
                HorizontalBackPorch = 2,
                HorizontalSyncPulseWidth = 41,
                HorizontalSyncPolarity = false,
                VerticalFrontPorch = 2,
                VerticalBackPorch = 2,
                VerticalSyncPulseWidth = 10,
                VerticalSyncPolarity = false,
            });

            displayController.Enable();
            var screen = Graphics.FromHdc(displayController.Hdc);  
            var font = Resources.GetFont(Resources.FontResources.ArialBlack32);
            var bigBitmapText = new Bitmap(480, 272 * 8);
            var bigScreen = Graphics.FromImage(bigBitmapText);

            bigScreen.Clear();
            bigScreen.DrawTextInRect(prompt, 50, 0, 400, bigBitmapText.Height, Graphics.DrawTextAlignment.WordWrap, Color.White, font);

            for (int y = 272; y > -272 * 9; y--){
                screen.Clear();
                screen.DrawImage(bigBitmapText, 0, y);

                BitConverter.SwapEndianness(screen.GetBitmap(), 480 * 272 * 2);
                BitConverter.SwapEndianness(screen.GetBitmap(), 480 * 2);
                screen.Flush();
                Thread.Sleep(speed);
            }
        }

    }
}