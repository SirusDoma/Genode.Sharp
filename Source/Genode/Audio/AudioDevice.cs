using System;
using System.Collections.Generic;
using System.Text;

using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

using Genode;
using Genode.Internal.OpenAL;

namespace Genode.Audio
{
    internal static class AudioDevice
    {
        private static AudioContext _context;
        private static float _listenerVolume;
        private static Vector3 _listenerPosition;
        private static Vector3 _listenerDirection;
        private static Vector3 _listenerUpVector;

        public static bool IsDisposed
        {
            get;
            set;
        }

        public static float Volume
        {
            get
            {
                return _listenerVolume;
            }
            set
            {
                ALChecker.Check(() => AL.Listener(ALListenerf.Gain, value * 0.01f));
                _listenerVolume = value;
            }
        }

        public static Vector3 Position
        {
            get
            {
                return _listenerPosition;
            }
            set
            {
                ALChecker.Check(() => AL.Listener(ALListener3f.Position, value.X, value.Y, value.Z));
                _listenerPosition = value;
            }
        }

        public static Vector3 Direction
        {
            get
            {
                return _listenerDirection;
            }
            set
            {
                float[] orientation = {value.X,
                                       value.Y,
                                       value.Z,
                                       _listenerUpVector.X,
                                       _listenerUpVector.Y,
                                       _listenerUpVector.Z};

                ALChecker.Check(() => AL.Listener(ALListenerfv.Orientation, ref orientation));
                _listenerDirection = value;
            }
        }

        public static Vector3 UpVector
        {
            get
            {
                return _listenerDirection;
            }
            set
            {
                float[] orientation = {_listenerDirection.X,
                                       _listenerDirection.Y,
                                       _listenerDirection.Z,
                                       value.X,
                                       value.Y,
                                       value.Z};

                ALChecker.Check(() => AL.Listener(ALListenerfv.Orientation, ref orientation));
                _listenerUpVector = value;
            }
        }

        static AudioDevice()
        {
            // Create audio context (which create the ALCdevice and ALCcontext)
            _context = new AudioContext();
            _context.MakeCurrent();

            // Configure default state of listener
            _listenerVolume = 100f;
            _listenerPosition  = new Vector3(0f, 0f,  0f);
            _listenerDirection = new Vector3(0f, 0f, -1f);
            _listenerUpVector  = new Vector3(0f, 1f,  0f);


            // Apply the listener properties the user might have set
            float[] orientation = {_listenerDirection.X,
                                   _listenerDirection.Y,
                                   _listenerDirection.Z,
                                   _listenerUpVector.X,
                                   _listenerUpVector.Y,
                                   _listenerUpVector.Z};

            ALChecker.Check(() => AL.Listener(ALListenerf.Gain, _listenerVolume * 0.01f));
            ALChecker.Check(() => AL.Listener(ALListener3f.Position, _listenerPosition.X, _listenerPosition.Y, _listenerPosition.Z));
            ALChecker.Check(() => AL.Listener(ALListenerfv.Orientation, ref orientation));

            // Dispose Audio Device when exiting application
            AppDomain.CurrentDomain.ProcessExit += (s, e) => Dispose();
        }

        public static void Initialize()
        {
            // Do nothing, Audio Device is initialized in static constructor
            // Therefore, the constructor is called whenever any static members / methods is called within this class
            // This ensure the initialization only happen once without maintaining any flags
            // In other words, this is only convinent class to trigger initialization
        }

        public static bool IsExtensionSupported(string extension)
        {
            return AL.IsExtensionPresent(extension);
        }

        public static ALFormat GetFormat(int channelCount)
        {
            ALFormat format = 0;
            switch (channelCount)
            {
                case 1:  format = ALFormat.Mono16;          break;
                case 2:  format = ALFormat.Stereo16;        break;
                case 4:  format = ALFormat.MultiQuad16Ext;  break;
                case 6:  format = ALFormat.Multi51Chn16Ext; break;
                case 7:  format = ALFormat.Multi61Chn16Ext; break;
                case 8:  format = ALFormat.Multi71Chn16Ext; break;
                default: format = 0;                        break;
            }
            
            // Fixes a bug on OS X
            if ((int)format == -1)
                format = 0;

            return format;
        }

        public static void Dispose()
        {
            if (_context != null)
            {
                _context.Dispose();
                _context = null;

                IsDisposed = true;
            }
        }
    }
}
