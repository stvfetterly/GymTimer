using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GymTimer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        enum timeType { ROUND, REST };
        timeType _currentRound = timeType.ROUND;

        MediaPlayer _mediaPlayer = new MediaPlayer();

        Uri _roundSoundPath;
        Uri _restSoundPath;
        Uri _warnSoundPath;

        DispatcherTimer _timer;

        int _secondsLeft;
        int _secondsRest;
        int _secondsRound;

        bool _roundEndWarning = true;       //by default the round end warning is on


        public MainWindow()
        {
            InitializeComponent();

            //Set up the paths to the default sounds
            string curDir = Environment.CurrentDirectory;
            _roundSoundPath = new Uri(curDir + "\\Sounds\\ThreeDings.mp3", UriKind.Relative);
            _restSoundPath = new Uri(curDir + "\\Sounds\\SingleDing.mp3", UriKind.Relative);
            _warnSoundPath = new Uri(curDir + "\\Sounds\\TimeWarning.mp3", UriKind.Relative);

            //Default 30 seconds rest, 2:30 round
            _secondsRest = 30;
            _secondsRound = 150;

            //Creates the timer
            _timer = new DispatcherTimer();
            _timer.IsEnabled = true;
            _timer.Tick += new EventHandler(timer_Tick);
            _timer.Interval = TimeSpan.FromSeconds(1);      //Tick every second
            _secondsLeft = 0;

            //Add items to minute and second comboboxes
            for (int i = 0; i < 60; i = i + 1)
            {
                cmb_RestTimeMin.Items.Add(i);
                cmb_RestTimeSec.Items.Add(i);
                cmb_RoundTimeMin.Items.Add(i);
                cmb_RoundTimeSec.Items.Add(i);
            }

            //default to 30 second rest
            cmb_RestTimeMin.SelectedItem = 0;
            cmb_RestTimeSec.SelectedItem = 30;

            //default to 2:30 second round
            cmb_RoundTimeMin.SelectedItem = 2;
            cmb_RoundTimeSec.SelectedItem = 30;

            //volume is set to 100 by default
            sld_volume.Value = 100;
        }

        //Updates the displayed time left
        private void timer_Tick(object sender, EventArgs e)
        {
            if (_secondsLeft > 0)
            {
                _secondsLeft = _secondsLeft - 1;
            }
            
            updateTime();

            //Once the time left reaches zero, we start a new round (either a rest one or another regular round)
            if (_secondsLeft == 0)
            {
                //Check if we should do a rest. If so, start rest time.
                if (_currentRound == timeType.ROUND && cmb_RestTimeMin.IsEnabled)
                {
                    _currentRound = timeType.REST;
                    startTimer();
                }
                else
                {
                    //otherwise start a new round
                    _currentRound = timeType.ROUND;
                    startTimer();
                }
            }
        }

        //Updates the time remaining displayed to user
        private void updateTime()
        {
            int min = _secondsLeft / 60;
            int sec = _secondsLeft % 60;

            if (sec <= 9)
            {
                lbl_Time.Content = min + ":0" + sec;
            }
            else
            {
                lbl_Time.Content = min + ":" + sec;
            }

            updateTimeColour();
        }

        private void updateTimeColour()
        {

            LinearGradientBrush redGradient = new LinearGradientBrush();
            redGradient.GradientStops.Add(new GradientStop(Colors.Tomato, 0.0));
            redGradient.GradientStops.Add(new GradientStop(Colors.Red, 0.5));
            redGradient.GradientStops.Add(new GradientStop(Colors.Tomato, 1.0));

            LinearGradientBrush greenGradient = new LinearGradientBrush();
            greenGradient.GradientStops.Add(new GradientStop(Colors.Chartreuse, 0.0));
            greenGradient.GradientStops.Add(new GradientStop(Colors.Green, 0.5));
            greenGradient.GradientStops.Add(new GradientStop(Colors.Chartreuse, 1.0));

            LinearGradientBrush blueGradient = new LinearGradientBrush();
            blueGradient.GradientStops.Add(new GradientStop(Colors.CornflowerBlue, 0.0));
            blueGradient.GradientStops.Add(new GradientStop(Colors.Blue, 0.5));
            blueGradient.GradientStops.Add(new GradientStop(Colors.CornflowerBlue, 1.0));

            LinearGradientBrush whiteGradient = new LinearGradientBrush();
            whiteGradient.GradientStops.Add(new GradientStop(Colors.White, 0.0));
            whiteGradient.GradientStops.Add(new GradientStop(Colors.Gray, 0.5));
            whiteGradient.GradientStops.Add(new GradientStop(Colors.White, 1.0));

            //Update the main timer - if we are within 10% of the end, display special colour
            if (_currentRound == timeType.REST)
            {
                //If we're within 10 s of the end time for a rest period, display BLUE as warning
                if (_secondsLeft <= 10)
                {
                    lbl_Time.Foreground = blueGradient;
                    roundEndWarning();
                }
                //Otherwise display GREEN
                else
                {
                    lbl_Time.Foreground = greenGradient;
                }
            }
            else
            {
                //If we're within 15s of the end time for a rest period, display RED as warning
                if (_secondsLeft <= 10)
                {
                    lbl_Time.Foreground = redGradient;
                
                    roundEndWarning();
                }
                //Otherwise display WHITE
                else
                {
                    lbl_Time.Foreground = whiteGradient;
                }
            }
        }

        //plays a warning sound
        private void roundEndWarning()
        {
            if (_roundEndWarning)
            {
                _mediaPlayer.Stop();
                _mediaPlayer.Open(_warnSoundPath);
                _mediaPlayer.Play();
            }
        }

        //Enables/Disables the rest time period
        private void chk_RestTime_Click(object sender, RoutedEventArgs e)
        {
            cmb_RestTimeMin.IsEnabled = (bool)chk_RestTime.IsChecked;
            cmb_RestTimeSec.IsEnabled = (bool)chk_RestTime.IsChecked;
            label_Copy.IsEnabled = (bool)chk_RestTime.IsChecked;
            label3.IsEnabled = (bool)chk_RestTime.IsChecked;

            resetGymTimer();
        }

        private void btn_Start_Click(object sender, RoutedEventArgs e)
        {
            //Timer is running already and must be stopped
            if (_secondsLeft > 0 && (string)btn_Start.Content == "Stop")
            {
                _timer.Stop();
                btn_Start.Content = "Start";
            }
            //Timer is stopped and must be started
            else
            {
                startTimer();
                btn_Start.Content = "Stop";
            }
        }

        //Starts a new round or restarts a paused one
        private void startTimer()
        {
            int seconds;            //keeps track of the number of seconds the user has selected for timer
            _mediaPlayer.Stop();    //Otherwise each time you open a sound path media player will play it

            if (_currentRound == timeType.ROUND)
            {
                seconds = _secondsRound;
                _mediaPlayer.Open(_roundSoundPath);
            }
            else
            {
                seconds = _secondsRest;
                _mediaPlayer.Open(_restSoundPath);
            }

            //If we have paused, we just continue.  Otherwise count down from the user selected amount of time.
            if (_secondsLeft == 0)
            {
                _secondsLeft = seconds;

                //play the sound for starting a new round
                _mediaPlayer.Play();
            }

            _timer.Start();
        }

        //pause the timer for a moment
        private void pauseTimer()
        {
            //Timer stops running, everything else stays the same
            _timer.Stop();
        }

        //Resets everything to default start conditions
        private void resetGymTimer()
        {
            //Stop any sounds that are playing
            _mediaPlayer.Stop();

            //Timer stops running
            _timer.Stop();

            _secondsLeft = 0;
            btn_Start.Content = "Start";

            //Update the round and rest seconds
            if (cmb_RoundTimeMin.SelectedValue != null && 
                cmb_RoundTimeSec.SelectedValue != null && 
                cmb_RestTimeMin.SelectedValue != null && 
                cmb_RestTimeSec.SelectedValue != null)
            {
                _secondsRound = (60 * (int)cmb_RoundTimeMin.SelectedValue) + (int)cmb_RoundTimeSec.SelectedValue;
                _secondsRest = (60 * (int)cmb_RestTimeMin.SelectedValue) + (int)cmb_RestTimeSec.SelectedValue;

                updateTime();
            }
            
            //By default, the round starts
            _currentRound = timeType.ROUND;
        }

        //Mute or unmute the sound
        private void chk_Mute_Click(object sender, RoutedEventArgs e)
        {
            _mediaPlayer.IsMuted = !_mediaPlayer.IsMuted;
        }

        //Any time the user changes the time, stop anything that is currently running
        private void cmb_RoundTimeMin_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            resetGymTimer();
        }
        private void cmb_RoundTimeSec_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            resetGymTimer();
        }
        private void cmb_RestTimeMin_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            resetGymTimer();
        }
        private void cmb_RestTimeSec_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            resetGymTimer();
        }

        private void btn_Reset_Click(object sender, RoutedEventArgs e)
        {
            resetGymTimer();
        }

        //Toggles end of round warning on or off
        private void chk_RoundWarning_Click(object sender, RoutedEventArgs e)
        {
            _roundEndWarning = (bool)chk_RoundWarning.IsChecked;
        }

        private void sld_volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            float newVol = (float)sld_volume.Value / 100;
            _mediaPlayer.Volume = newVol;
        }
    }
}
