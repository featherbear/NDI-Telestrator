using NewTek;
using NewTek.NDI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NewTek.NDI.WPF
{
    // If you do not use this control, you can remove this file
    // and remove the dependency on naudio.
    // Alternatively you can also remove any naudio related entries
    // and use it for video only, but don't forget that you will still need
    // to free any audio frames received.
    public class ReceiveView : Viewbox, IDisposable, INotifyPropertyChanged
    {
        [Category("NewTek NDI"),
        Description("The name of this receiver channel. Required or else an invalid argument exception will be thrown.")]
        public String ReceiverName
        {
            get { return (String)GetValue(ReceiverNameProperty); }
            set { SetValue(ReceiverNameProperty, value); }
        }
        public static readonly DependencyProperty ReceiverNameProperty =
            DependencyProperty.Register("ReceiverName", typeof(String), typeof(ReceiveView), new PropertyMetadata(""));

        

        [Category("NewTek NDI"),
        Description("The NDI source to connect to. An empty new Source() or a Source with no Name will disconnect.")]
        public Source ConnectedSource
        {
            get { return (Source)GetValue(ConnectedSourceProperty); }
            set { SetValue(ConnectedSourceProperty, value); }
        }
        public static readonly DependencyProperty ConnectedSourceProperty =
            DependencyProperty.Register("ConnectedSource", typeof(Source), typeof(ReceiveView), new PropertyMetadata(new Source(), OnConnectedSourceChanged));

        [Category("NewTek NDI"),
        Description("If true (default) received video will be sent to the screen.")]
        public bool IsVideoEnabled
        {
            get { return _videoEnabled; }
            set
            {
                if (value != _videoEnabled)
                {
                    NotifyPropertyChanged("IsVideoEnabled");
                }
            }
        }


        [Category("NewTek NDI"),
        Description("Does the current source support record functionality?")]
        public bool IsRecordingSupported
        {
            get { return _canRecord; }
            set
            {
                if (value != _canRecord)
                {
                    NotifyPropertyChanged("IsRecordingSupported");
                }
            }
        }

        [Category("NewTek NDI"),
        Description("The web control URL for the current device, as a String, or an Empty String if not supported.")]
        public String WebControlUrl
        {
            get { return _webControlUrl; }
            set
            {
                if (value != _webControlUrl)
                {
                    NotifyPropertyChanged("WebControlUrl");
                }
            }
        }

        public ReceiveView()
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                return;
        }

        public event PropertyChangedEventHandler PropertyChanged;


#region Recording Methods
    // This will start recording.If the recorder was already recording then the message is ignored.A filename is passed in as a ‘hint’.Since the recorder might 
	// already be recording(or might not allow complete flexibility over its filename), the filename might or might not be used.If the filename is empty, or 
	// not present, a name will be chosen automatically. 
    public bool RecordingStart(String filenameHint = "")
    {
        if (!_canRecord || _recvInstancePtr == IntPtr.Zero)
                return false;

        bool retVal = false;

        if(String.IsNullOrEmpty(filenameHint))
        {
            retVal = NDIlib.recv_recording_start(_recvInstancePtr, IntPtr.Zero);
        }
        else
        {
            // convert to an unmanaged UTF8 IntPtr
            IntPtr fileNamePtr = NDI.UTF.StringToUtf8(filenameHint);

            retVal = NDIlib.recv_recording_start(_recvInstancePtr, IntPtr.Zero);

            // don't forget to free it
            Marshal.FreeHGlobal(fileNamePtr);            
        }

        return retVal;
    }

	// Stop recording.
	public bool RecordingStop()
    {
        if (!_canRecord || _recvInstancePtr == IntPtr.Zero)
                return false;

	    return NDIlib.recv_recording_stop(_recvInstancePtr);
    }

    
    public bool RecordingSetAudioLevel(double level)
    {
        if (!_canRecord || _recvInstancePtr == IntPtr.Zero)
                return false;

        return NDIlib.recv_recording_set_audio_level(_recvInstancePtr, (float)level);
    }

    public bool IsRecording()
    {
        if (!_canRecord || _recvInstancePtr == IntPtr.Zero)
                return false;

        return NDIlib.recv_recording_is_recording(_recvInstancePtr);
    }

    public String GetRecordingFilename()
    {
        if (!_canRecord || _recvInstancePtr == IntPtr.Zero)
                return String.Empty;

        IntPtr filenamePtr = NDIlib.recv_recording_get_filename(_recvInstancePtr);
        if(filenamePtr == IntPtr.Zero)
        {
            return String.Empty;
        }
        else
        {
            String filename = NDI.UTF.Utf8ToString(filenamePtr);

            // free it
            NDIlib.recv_free_string(_recvInstancePtr, filenamePtr);

            return filename;
        }
    }

    public String GetRecordingError()
    {
        if (!_canRecord || _recvInstancePtr == IntPtr.Zero)
            return String.Empty;

        IntPtr errorPtr = NDIlib.recv_recording_get_error(_recvInstancePtr);
        if (errorPtr == IntPtr.Zero)
        {
            return String.Empty;
        }
        else
        {
            String error = NDI.UTF.Utf8ToString(errorPtr);

            // free it
            NDIlib.recv_free_string(_recvInstancePtr, errorPtr);

            return error;
        }
    }

    public bool GetRecordingTimes(ref NDIlib.recv_recording_time_t recordingTimes)
    {
        if (!_canRecord || _recvInstancePtr == IntPtr.Zero)
            return false;

        return NDIlib.recv_recording_get_times(_recvInstancePtr, ref recordingTimes);
    }

#endregion Recording Methods

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ReceiveView()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // tell the thread to exit
                    _exitThread = true;

                    // wait for it to exit
                    if (_receiveThread != null)
                    {
                        _receiveThread.Join();

                        _receiveThread = null;
                    }
                }
                
                // Destroy the receiver
                if (_recvInstancePtr != IntPtr.Zero)
                {
                    NDIlib.recv_destroy(_recvInstancePtr);
                    _recvInstancePtr = IntPtr.Zero;
                }
                
                // Not required, but "correct". (see the SDK documentation)
                NDIlib.destroy();

                _disposed = true;
            }
        }

        private bool _disposed = false;

        // when the ConnectedSource changes, connect to it.
        private static void OnConnectedSourceChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ReceiveView s = sender as ReceiveView;
            if (s == null)
                return;

            s.Connect(s.ConnectedSource);
        }

        // connect to an NDI source in our Dictionary by name
        private void Connect(Source source)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                return;

            // before we are connected, we need to set up our image
            // it's bad practice to do this in the constructor
            if (Child == null)
                Child = VideoSurface;

            // just to be safe
            Disconnect();
            
            // Sanity
            if (source == null || String.IsNullOrEmpty(source.Name))
                return;

            if (String.IsNullOrEmpty(ReceiverName))
                throw new ArgumentException("ReceiverName can not be null or empty.", ReceiverName);

            // a source_t to describe the source to connect to.
            NDIlib.source_t source_t = new NDIlib.source_t()
            {
                p_ndi_name = UTF.StringToUtf8(source.Name)
            };

            // make a description of the receiver we want
            NDIlib.recv_create_v3_t recvDescription = new NDIlib.recv_create_v3_t()
            {
                // the source we selected
                source_to_connect_to = source_t,

                // we want BGRA frames for this example
                color_format = NDIlib.recv_color_format_e.recv_color_format_BGRX_BGRA,

                // we want full quality - for small previews or limited bandwidth, choose lowest
                bandwidth = NDIlib.recv_bandwidth_e.recv_bandwidth_highest,

                // let NDIlib deinterlace for us if needed
                allow_video_fields = false,

                // The name of the NDI receiver to create. This is a NULL terminated UTF8 string and should be
                // the name of receive channel that you have. This is in many ways symettric with the name of
                // senders, so this might be "Channel 1" on your system.
                p_ndi_recv_name = UTF.StringToUtf8(ReceiverName)
            };
            
            // create a new instance connected to this source
            _recvInstancePtr = NDIlib.recv_create_v3(ref recvDescription);

            // free the memory we allocated with StringToUtf8
            Marshal.FreeHGlobal(source_t.p_ndi_name);
            Marshal.FreeHGlobal(recvDescription.p_ndi_recv_name);

            // did it work?
            System.Diagnostics.Debug.Assert(_recvInstancePtr != IntPtr.Zero, "Failed to create NDI receive instance.");

            if (_recvInstancePtr != IntPtr.Zero)
            {
                // We are now going to mark this source as being on program output for tally purposes (but not on preview)
                SetTallyIndicators(true, false);

                // start up a thread to receive on
                _receiveThread = new Thread(ReceiveThreadProc) { IsBackground = true, Name = "NdiExampleReceiveThread" };
                _receiveThread.Start();
            }
        }

        public void Disconnect()
        {
            // in case we're connected, reset the tally indicators
            SetTallyIndicators(false, false);

            // check for a running thread
            if (_receiveThread != null)
            {
                // tell it to exit
                _exitThread = true;

                // wait for it to end
                _receiveThread.Join();
            }

            // reset thread defaults
            _receiveThread = null;
            _exitThread = false;

            // Destroy the receiver
            NDIlib.recv_destroy(_recvInstancePtr);

            // set it to a safe value
            _recvInstancePtr = IntPtr.Zero;

            // set function status to defaults
            IsRecordingSupported = false;
            WebControlUrl = String.Empty;
        }

        void SetTallyIndicators(bool onProgram, bool onPreview)
        {
            // we need to have a receive instance
            if (_recvInstancePtr != IntPtr.Zero)
            {
                // set up a state descriptor
                NDIlib.tally_t tallyState = new NDIlib.tally_t()
                {
                    on_program = onProgram,
                    on_preview = onPreview
                };

                // set it on the receiver instance
                NDIlib.recv_set_tally(_recvInstancePtr, ref tallyState);
            }
        }

        // the receive thread runs though this loop until told to exit
        void ReceiveThreadProc()
        {
            while (!_exitThread && _recvInstancePtr != IntPtr.Zero)
            {
                // The descriptors
                NDIlib.video_frame_v2_t videoFrame = new NDIlib.video_frame_v2_t();
                NDIlib.audio_frame_v2_t audioFrame = new NDIlib.audio_frame_v2_t();
                NDIlib.metadata_frame_t metadataFrame = new NDIlib.metadata_frame_t();

                switch (NDIlib.recv_capture_v2(_recvInstancePtr, ref videoFrame, ref audioFrame, ref metadataFrame, 1000))
                {
                // No data
                case NDIlib.frame_type_e.frame_type_none:
                    // No data received
                    break;

                // frame settings - check for extended functionality
                case NDIlib.frame_type_e.frame_type_status_change:

                    // Check for recording
                    IsRecordingSupported = NDIlib.recv_recording_is_supported(_recvInstancePtr);

                    // Check for a web control URL
                    // We must free this string ptr if we get one.
                    IntPtr webUrlPtr = NDIlib.recv_get_web_control(_recvInstancePtr);
                    if (webUrlPtr == IntPtr.Zero)
                    {
                        WebControlUrl = String.Empty;
                    }
                    else
                    {
                        // convert to managed String
                        WebControlUrl = NDI.UTF.Utf8ToString(webUrlPtr);

                        // Don't forget to free the string ptr
                        NDIlib.recv_free_string(_recvInstancePtr, webUrlPtr);
                    }

                    break;

                // Video data
                case NDIlib.frame_type_e.frame_type_video:

                    // if not enabled, just discard
                    // this can also occasionally happen when changing sources
                    if (!_videoEnabled || videoFrame.p_data == IntPtr.Zero)
                    {
                        // alreays free received frames
                        NDIlib.recv_free_video_v2(_recvInstancePtr, ref videoFrame);

                        break;
                    }

                    // get all our info so that we can free the frame
                    int yres = (int)videoFrame.yres;
                    int xres = (int)videoFrame.xres;

                    // quick and dirty aspect ratio correction for non-square pixels - SD 4:3, 16:9, etc.
                    double dpiX = 96.0 * (videoFrame.picture_aspect_ratio / ((double)xres / (double)yres));

                    int stride = (int)videoFrame.line_stride_in_bytes;
                    int bufferSize = yres * stride;

                    // We need to be on the UI thread to write to our bitmap
                    // Not very efficient, but this is just an example
                    Dispatcher.BeginInvoke(new Action(delegate
                    {
                        // resize the writeable if needed
                        if (VideoBitmap == null ||
                            VideoBitmap.PixelWidth != xres ||
                            VideoBitmap.PixelHeight != yres ||
                            VideoBitmap.DpiX != dpiX)
                        {
                            VideoBitmap = new WriteableBitmap(xres, yres, dpiX, 96.0, PixelFormats.Pbgra32, null);
                            VideoSurface.Source = VideoBitmap;
                        }

                        // update the writeable bitmap
                        VideoBitmap.WritePixels(new Int32Rect(0, 0, xres, yres), videoFrame.p_data, bufferSize, stride);

                        // free frames that were received AFTER use!
                        // This writepixels call is dispatched, so we must do it inside this scope.
                        NDIlib.recv_free_video_v2(_recvInstancePtr, ref videoFrame);
                    }));

                    break;

                // Metadata
                case NDIlib.frame_type_e.frame_type_metadata:

                    // UTF-8 strings must be converted for use - length includes the terminating zero
                    //String metadata = Utf8ToString(metadataFrame.p_data, metadataFrame.length-1);

                    //System.Diagnostics.Debug.Print(metadata);

                    // free frames that were received
                    NDIlib.recv_free_metadata(_recvInstancePtr, ref metadataFrame);
                    break;
                }
            }
        }

        // a pointer to our unmanaged NDI receiver instance
        IntPtr _recvInstancePtr = IntPtr.Zero;

        // a thread to receive frames on so that the UI is still functional
        Thread _receiveThread = null;

        // a way to exit the thread safely
        bool _exitThread = false;

        // the image that will show our bitmap source
        private Image VideoSurface = new Image();

        // the bitmap source we copy received frames into
        private WriteableBitmap VideoBitmap;

        // should we send video to Windows or not?
        private bool _videoEnabled = true;

        private bool _canRecord = false;
        private String _webControlUrl = String.Empty;
        private String _receiverName = String.Empty;
    }
}
