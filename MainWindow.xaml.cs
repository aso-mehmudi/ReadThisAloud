using System;
using System.Windows;
using System.ComponentModel;
using System.Speech.Synthesis;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;
using System.Threading;

namespace ReadThisAloud
{
    public partial class MainWindow : Window
    {
        SpeechSynthesizer? synth;
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

        private void InitializeSynthsizer()
        {            
            synth = new SpeechSynthesizer();
            synth.SpeakCompleted += Synthsizer_SpeakCompleted;
            synth.StateChanged += Synthsizer_StateChanged;
        }

        public void speak(string text)
        {
            
            synth?.Dispose();
            player.Play();
            player.MediaEnded += new EventHandler(Media_Ended);
            foreach (var replace in replacements)
                text = Regex.Replace(text, replace.Key, replace.Value);
            
            Thread.Sleep(500); // wait a little before reading the text to avoid missing the first word on bluetooth headphones
            InitializeSynthsizer();
            synth.SpeakAsync(text);
        }

        private void Synthsizer_StateChanged(object? sender, StateChangedEventArgs e)
        {
            switch (e.State)
            {
                case SynthesizerState.Paused:
                    synth?.Pause();
                    break;
            }
        }

        private void Synthsizer_SpeakCompleted(object? sender, SpeakCompletedEventArgs e)
        {               
            StopAll();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            StopAll();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            StopAll();
        }

        public void StopAll()
        {            
            if (synth != null)
            {
                synth.Dispose();
                synth = null;
            } 
            player.Stop();
            IsPlaying = false;
            btnRun.Visibility = Visibility.Visible;
            btnStop.Visibility = Visibility.Collapsed;
            btnPause.Visibility = Visibility.Collapsed;
        }

        private void Media_Ended(object sender, EventArgs e)
        {
            player.Position = TimeSpan.Zero;
            player.Play();
        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            speak(Clipboard.GetText());
            btnRun.Visibility = Visibility.Collapsed;
            btnStop.Visibility = Visibility.Visible;
            btnPause.Visibility = Visibility.Visible;
            IsPlaying = true;
        }
        
        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            if (IsPlaying){
                synth?.Pause();                
                btnPause.Content = "▶️";
                player.Pause();
            }
            else{
                player.Play();
                btnPause.Content = "⏸️";
                Thread.Sleep(500);
                synth?.Resume();
            }
            IsPlaying = !IsPlaying;
        }

        private void btnRun_Drop(object sender, DragEventArgs e)
        {
            speak(e.Data.GetData("Text") as string);
            btnRun.Visibility = Visibility.Collapsed;
            btnStop.Visibility = Visibility.Visible;
            btnPause.Visibility = Visibility.Visible;
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
