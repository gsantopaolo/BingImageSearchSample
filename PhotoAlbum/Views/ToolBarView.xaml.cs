using PhotoAlbum.ViewModels;
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
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PhotoAlbum.Views
{
    public sealed partial class ToolBarView : UserControl
    {
        // strongly-typed view models enable x:bind
        public ToolBarViewModel ViewModel => this.DataContext as ToolBarViewModel;

        public ToolBarView()
        {
            this.InitializeComponent();
        }

        private SpeechRecognizer speechRecognizer;
        private CoreDispatcher dispatcher;
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

    public class AudioCapturePermissions
    {
        // If no recording device is attached, attempting to get access to audio capture devices will throw 
        // a System.Exception object, with this HResult set.
        private static int NoCaptureDevicesHResult = -1072845856;

        /// <summary>
        /// On desktop/tablet systems, users are prompted to give permission to use capture devices on a 
        /// per-app basis. Along with declaring the microphone DeviceCapability in the package manifest,
        /// this method tests the privacy setting for microphone access for this application.
        /// Note that this only checks the Settings->Privacy->Microphone setting, it does not handle
        /// the Cortana/Dictation privacy check, however (Under Settings->Privacy->Speech, Inking and Typing).
        /// 
        /// Developers should ideally perform a check like this every time their app gains focus, in order to 
        /// check if the user has changed the setting while the app was suspended or not in focus.
        /// </summary>
        /// <returns>true if the microphone can be accessed without any permissions problems.</returns>
        public async static Task<bool> RequestMicrophonePermission()
        {
            try
            {
                // Request access to the microphone only, to limit the number of capabilities we need
                // to request in the package manifest.
                MediaCaptureInitializationSettings settings = new MediaCaptureInitializationSettings();
                settings.StreamingCaptureMode = StreamingCaptureMode.Audio;
                settings.MediaCategory = MediaCategory.Speech;
                MediaCapture capture = new MediaCapture();

                await capture.InitializeAsync(settings);
            }
            catch (TypeLoadException)
            {
                // On SKUs without media player (eg, the N SKUs), we may not have access to the Windows.Media.Capture
                // namespace unless the media player pack is installed. Handle this gracefully.
                var messageDialog = new Windows.UI.Popups.MessageDialog("Media player components are unavailable.");
                await messageDialog.ShowAsync();
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                // The user has turned off access to the microphone. If this occurs, we should show an error, or disable
                // functionality within the app to ensure that further exceptions aren't generated when 
                // recognition is attempted.
                return false;
            }
            catch (Exception exception)
            {
                // This can be replicated by using remote desktop to a system, but not redirecting the microphone input.
                // Can also occur if using the virtual machine console tool to access a VM instead of using remote desktop.
                if (exception.HResult == NoCaptureDevicesHResult)
                {
                    var messageDialog = new Windows.UI.Popups.MessageDialog("No Audio Capture devices are present on this system.");
                    await messageDialog.ShowAsync();
                    return false;
                }
                else
                {
                    throw;
                }
            }
            return true;
        }
    }
}
