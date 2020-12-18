using System;
using System.Windows;
using System.ComponentModel;
using System.Speech.Synthesis;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;

namespace ReadThisAloud
{
    public partial class MainWindow : Window
    {
        SpeechSynthesizer synth = new SpeechSynthesizer();
        System.Windows.Media.MediaPlayer player;
        Dictionary<string, string> replacements = new Dictionary<string, string>();

        public bool IsPlaying { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            player = new System.Windows.Media.MediaPlayer();
            player.Volume = 0.25;

            if (File.Exists("replacements.txt"))
            {
                foreach (var line in File.ReadAllLines("replacements.txt"))
                {
                    var l = Regex.Match(line, "\"(.+)\"\\s+=>\\s+\"(.*)\"");
                    if (l.Length > 0)
                    {
                        var rFrom = l.Groups[1].Value;
                        var rTo = l.Groups[2].Value;
                        if (!replacements.ContainsKey(rFrom))
                            replacements.Add(rFrom, rTo);
                    }
                }
            }
            if (File.Exists("bg.wav"))
                player.Open(new Uri(Path.GetFullPath("bg.wav")));
        }

        public void speak(string text)
        {
            player.Play();
            player.MediaEnded += new EventHandler(Media_Ended);
            synth.SpeakAsyncCancelAll();
            foreach (var replace in replacements)
                text = Regex.Replace(text, replace.Key, replace.Value);
            synth.SpeakAsync(text);
            synth.SpeakCompleted += new EventHandler<SpeakCompletedEventArgs>(stop);
        }
        public void stop(object sender, SpeakCompletedEventArgs e)
        {
            PlayPath.Visibility = Visibility.Visible;
            StopPath.Visibility = Visibility.Collapsed;
            player.Stop();
        }
        private void Media_Ended(object sender, EventArgs e)
        {
            player.Position = TimeSpan.Zero;
            player.Play();
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
            MessageBox.Show(@"
This program starts Microsoft Windows default text-to-speech
by drag and dropping texts or copying texts and clicking the button.

Put a wave audio file in application folder for playing in background.

Developed by Aso Mahmudi: https://github.com/aso-mehmudi",
                "Read This Aloud", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
