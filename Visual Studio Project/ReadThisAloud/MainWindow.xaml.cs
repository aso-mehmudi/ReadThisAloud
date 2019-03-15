using System;
using System.Windows;
using System.ComponentModel;
using System.Speech.Synthesis;
using System.Text.RegularExpressions;
namespace ReadThisAloud
{
    /// <summary>
    /// Developed by "Aso Mahmudi" (aso.mehmudi@gmail.com)
    /// https://github.com/aso-mehmudi
    /// </summary>
    public partial class MainWindow : Window
    {
        SpeechSynthesizer synth = new SpeechSynthesizer();
        public bool IsPlaying { get; set; }
        public MainWindow()
        {
            InitializeComponent();
        }

        public void speak(string text)
        {
            synth.SpeakAsyncCancelAll();
            var replaces = new string[] {
                "([^.!?])\r?\n", "$1 ",
                " {2,}", " ",
                @"\[\d+\]", " ",
                @"et al\.?", "and others",
                " per cent", "%",
                "https?://", "",
                "-\r?\n", "",
            };
            for (int i = 0; i < replaces.Length; i += 2)
                text = Regex.Replace(text, replaces[i], replaces[i + 1]);
            synth.SpeakAsync(text);
            synth.SpeakCompleted += new EventHandler<SpeakCompletedEventArgs>(stop);
        }
        public void stop(object sender, SpeakCompletedEventArgs e)
        {
            PlayPath.Visibility = Visibility.Visible;
            StopPath.Visibility = Visibility.Collapsed;
        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            if (IsPlaying)
            {
                synth.SpeakAsyncCancelAll();
                PlayPath.Visibility = Visibility.Visible;
                StopPath.Visibility = Visibility.Collapsed;
            }
            else
            {
                speak(Clipboard.GetText());
                PlayPath.Visibility = Visibility.Collapsed;
                StopPath.Visibility = Visibility.Visible;
            }
            IsPlaying = !IsPlaying;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            synth.SpeakAsyncCancelAll();
        }

        private void btnRun_Drop(object sender, DragEventArgs e)
        {
            speak(e.Data.GetData("Text") as string);
            PlayPath.Visibility = Visibility.Collapsed;
            StopPath.Visibility = Visibility.Visible;
            IsPlaying = true;
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            var ControlPanelPath = System.IO.Path.Combine(Environment.SystemDirectory, "control.exe");
            System.Diagnostics.Process.Start(ControlPanelPath, "/name Microsoft.TextToSpeech");
        }

        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This program starts Microsoft Windows default text-to-speach \n by drag and dropoing texts \n or copying texts and clicking the button. \n\n Developed by Aso Mahmudi \n https://github.com/aso-mehmudi", "Read This Aloud", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
