using MPCExtensions.Common;
using BingImageSearchSample.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.SpeechRecognition;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace BingImageSearchSample.Views
{
    public sealed partial class ToolBarViewOLD : UserControl
    {
        // strongly-typed view models enable x:bind
        public ToolBarViewModel ViewModel => this.DataContext as ToolBarViewModel;

        public ToolBarViewOLD()
        {
            this.InitializeComponent();

            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromSeconds(3);

            InitializeInker();
            
        }

        private SpeechRecognizer speechRecognizer;
        private CoreDispatcher dispatcher;

        DispatcherTimer timer;

        /// <summary>
        /// When activating the scenario, ensure we have permission from the user to access their microphone, and
        /// provide an appropriate path for the user to enable access to the microphone if they haven't
        /// given explicit permission for it.
        /// </summary>
        /// <param name="e">The navigation event details</param>
        private async Task InitSpeech()
        {
            // Save the UI thread dispatcher to allow speech status messages to be shown on the UI.
            dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

            bool permissionGained = await AudioCapturePermissions.RequestMicrophonePermission();
            if (permissionGained)
            {
                // Enable the recognition buttons.
                button.IsEnabled = true;

                if (speechRecognizer != null)
                {
                    // cleanup prior to re-initializing this scenario.
                    //speechRecognizer.StateChanged -= SpeechRecognizer_StateChanged;

                    this.speechRecognizer.Dispose();
                    this.speechRecognizer = null;
                }

                // Create an instance of SpeechRecognizer.
                speechRecognizer = new SpeechRecognizer();

                // Provide feedback to the user about the state of the recognizer.
                //speechRecognizer.StateChanged += SpeechRecognizer_StateChanged;

                // Compile the dictation topic constraint, which optimizes for dictated speech.
                var dictationConstraint = new SpeechRecognitionTopicConstraint(SpeechRecognitionScenario.Dictation, "dictation");
                speechRecognizer.Constraints.Add(dictationConstraint);
                SpeechRecognitionCompilationResult compilationResult = await speechRecognizer.CompileConstraintsAsync();

                speechRecognizer.HypothesisGenerated += SpeechRecognizer_HypothesisGenerated;

                // Check to make sure that the constraints were in a proper format and the recognizer was able to compile it.
                if (compilationResult.Status != SpeechRecognitionResultStatus.Success)
                {
                    // Disable the recognition buttons.
                    button.IsEnabled = false;

                    // Let the user know that the grammar didn't compile properly.
                    //resultTextBlock.Visibility = Visibility.Visible;
                    //resultTextBlock.Text = "Unable to compile grammar.";
                }

            }
            else
            {
                // "Permission to access capture resources was not given by the user; please set the application setting in Settings->Privacy->Microphone.";
                button.IsEnabled = false;
            }

            await Task.Yield();
        }

        private void InitializeInker()
        {
            Inker.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Pen;

            var drawingAttributes = new InkDrawingAttributes
            {
                DrawAsHighlighter = false,
                Color = Colors.DarkBlue,
                PenTip = PenTipShape.Circle,
                IgnorePressure = false,
                Size = new Size(3, 3)
            };
            Inker.InkPresenter.UpdateDefaultDrawingAttributes(drawingAttributes);
            Inker.InkPresenter.StrokesCollected += InkPresenter_StrokesCollected;
        }

        private void InkPresenter_StrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
        {
            timer.Start();
        }

        private void Timer_Tick(object sender, object e)
        {
            timer.Stop();
            RecognizeInkerText();
        }

        private async Task RecognizeInkerText()
        {
            var inkRecognizer = new InkRecognizerContainer();
            var recognitionResults = await inkRecognizer.RecognizeAsync(Inker.InkPresenter.StrokeContainer, InkRecognitionTarget.All);

            List<TextBox> boxes = new List<TextBox>();

            foreach (var result in recognitionResults)
            {
                List<UIElement> elements = new List<UIElement>(
                    VisualTreeHelper.FindElementsInHostCoordinates(
                        new Rect(new Point(result.BoundingRect.X, result.BoundingRect.Y),
                        new Size(result.BoundingRect.Width, result.BoundingRect.Height)),
                    this));

                TextBox box = elements.Where(el => el is TextBox && (el as TextBox).IsEnabled).FirstOrDefault() as TextBox;

                if (box != null)
                {
                    if (!boxes.Contains(box))
                    {
                        boxes.Add(box);
                        box.Text = "";
                    }
                    box.Text +=  result.GetTextCandidates().FirstOrDefault().Trim();
                }
            }

            Inker.InkPresenter.StrokeContainer.Clear();
        }


        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await InitSpeech();

            // Disable the UI while recognition is occurring, and provide feedback to the user about current state.
            button.IsEnabled = false;
            text.IsReadOnly = true;

            text.Text = " listening for speech...";

            // Start recognition.
            try
            {
                //IAsyncOperation<SpeechRecognitionResult> recognitionOperation = speechRecognizer.RecognizeAsync();
                SpeechRecognitionResult speechRecognitionResult = await speechRecognizer.RecognizeAsync();
                //SpeechRecognitionResult speechRecognitionResult = await recognitionOperation;
                // If successful, display the recognition result.
                if (speechRecognitionResult.Status == SpeechRecognitionResultStatus.Success)
                {
                    text.Text = speechRecognitionResult.Text;
                }
                else
                {
                    text.Text = string.Format("Speech Recognition Failed, Status: {0}", speechRecognitionResult.Status.ToString());
                }
            }
            catch (TaskCanceledException exception)
            {
                // TaskCanceledException will be thrown if you exit the scenario while the recognizer is actively
                // processing speech. Since this happens here when we navigate out of the scenario, don't try to 
                // show a message dialog for this exception.
                System.Diagnostics.Debug.WriteLine("TaskCanceledException caught while recognition in progress (can be ignored):");
                System.Diagnostics.Debug.WriteLine(exception.ToString());
            }
            catch (Exception exception)
            {
                //// Handle the speech privacy policy error.
                //if ((uint)exception.HResult == HResultPrivacyStatementDeclined)
                //{
                //    hlOpenPrivacySettings.Visibility = Visibility.Visible;
                //}
                //else
                //{
                //    var messageDialog = new Windows.UI.Popups.MessageDialog(exception.Message, "Exception");
                //    await messageDialog.ShowAsync();
                //}
            }

            // Reset UI state.
            button.IsEnabled = true;
            text.IsReadOnly = false;
        }

        private async void SpeechRecognizer_HypothesisGenerated(SpeechRecognizer sender, SpeechRecognitionHypothesisGeneratedEventArgs args)
        {
            await text.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                text.Text = args.Hypothesis.Text;
            });
        }
    }

    
}
