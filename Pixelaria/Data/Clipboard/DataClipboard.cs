﻿/*
    Pixelaria
    Copyright (C) 2013 Luiz Fernando Silva

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along
    with this program; if not, write to the Free Software Foundation, Inc.,
    51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

    The full license may be found on the License.txt file attached to the
    base directory of this project.
*/

using System;
using System.Collections.Generic;
using System.IO;

namespace Pixelaria.Data.Clipboard
{
    /// <summary>
    /// Implements a simple clipboard system for the program
    /// </summary>
    public class DataClipboard
    {
        /// <summary>
        /// The currently stored clipboard data
        /// </summary>
        private IClipboardObject _currentData;

        /// <summary>
        /// Gets the type of the data currently stored in this ProgramClipboard
        /// </summary>
        public string CurrentDataType => _currentData == null ? "" : _currentData.GetDataType;

        /// <summary>
        /// The delegate for the clipboard events
        /// </summary>
        /// <param name="sender">The object that fired this event</param>
        /// <param name="eventArgs">The arguments for the event</param>
        public delegate void ClipboardEventHandler(object sender, ClipboardEventArgs eventArgs);

        /// <summary>
        /// Occurs whenever the contents of the clipboard change
        /// </summary>
        public event ClipboardEventHandler ClipboardChanged;

        /// <summary>
        /// Stores the given object on this clipboard.
        /// Setting a new object clears any object that was already on the clipboard
        /// </summary>
        /// <param name="dataObject">The new object to set to the clipboard</param>
        public void SetObject(IClipboardObject dataObject)
        {
            if (_currentData != null && _currentData != dataObject)
            {
                _currentData.Clear();
            }

            _currentData = dataObject;

            ClipboardChanged?.Invoke(this, new ClipboardEventArgs(_currentData, ClipboardEventType.Set));
        }

        /// <summary>
        /// Gets the object that corresponds to the current data of the clipboard
        /// </summary>
        /// <returns>The current data of the clipboard</returns>
        public IClipboardObject GetObject()
        {
            return _currentData;
        }

        /// <summary>
        /// Clears the contents of the clipboard
        /// </summary>
        public void Clear()
        {
            _currentData.Clear();
            _currentData = null;

            ClipboardChanged?.Invoke(this, new ClipboardEventArgs(null, ClipboardEventType.Clear));
        }
    }

    /// <summary>
    /// Arguments for a clipboard event
    /// </summary>
    public class ClipboardEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the object that was inputed into the clipboard
        /// </summary>
        public IClipboardObject NewObject { get; private set; }

        /// <summary>
        /// Gets the type of the event
        /// </summary>
        public ClipboardEventType EventType { get; private set; }

        /// <summary>
        /// Initializes a new class of the ClipboardEventArgs
        /// </summary>
        /// <param name="newObject">Gets the object that was inputed into the clipboard</param>
        /// <param name="eventType">Gets the type of the event</param>
        public ClipboardEventArgs(IClipboardObject newObject, ClipboardEventType eventType)
        {
            NewObject = newObject;
            EventType = eventType;
        }
    }

    /// <summary>
    /// Describes a type for a clipboard event
    /// </summary>
    public enum ClipboardEventType
    {
        /// <summary>
        /// A clipboard object was set
        /// </summary>
        Set,
        /// <summary>
        /// A clipboard object was cleared
        /// </summary>
        Clear
    }

    /// <summary>
    /// Specifies an object that stores data and can be used with a DataClipboard instance
    /// </summary>
    public interface IClipboardObject
    {
        /// <summary>
        /// Clears this clipboard object
        /// </summary>
        void Clear();

        /// <summary>
        /// Returns a string that specifies the data type of this ClipboardObject instance
        /// </summary>
        /// <returns>a string that specifies the data type of this ClipboardObject instance</returns>
        string GetDataType { get; }
    }

    /// <summary>
    /// Defines a ClipboardObject that contains a list of frames
    /// </summary>
    public class FrameListClipboardObject : IClipboardObject
    {
        /// <summary>
        /// The data type identifier for the FrameListclipboardObject
        /// </summary>
        public static readonly string DataType = "FrameList";

        /// <summary>
        /// List of frames to store into this ClipboardObject
        /// </summary>
        List<IFrame> _frameList;

        /// <summary>
        /// Gets the frames currently added to this FrameListClipboardObject instance
        /// </summary>
        public IFrame[] Frames => _frameList.ToArray();

        /// <summary>
        /// Initializes a new instance of the FrameListClipboardObject 
        /// </summary>
        public FrameListClipboardObject()
        {
            _frameList = new List<IFrame>();
        }

        /// <summary>
        /// Initializes a new FrameListClipboardObject with a list of frames to 
        /// </summary>
        /// <param name="frameList">A list of frames to initialize the internal frame list with</param>
        public FrameListClipboardObject(List<IFrame> frameList)
        {
            _frameList = new List<IFrame>(frameList);
        }

        /// <summary>
        /// Adds the given Frame to this FrameListClipboardObject instance
        /// </summary>
        /// <param name="frame">The frame to add</param>
        public void AddFrame(IFrame frame)
        {
            _frameList.Add(frame);
        }

        /// <summary>
        /// Clears this clipboard object
        /// </summary>
        public void Clear()
        {
            if (_frameList == null)
                return;

            foreach (var frame in _frameList)
            {
                frame.Dispose();
            }

            _frameList.Clear();
            _frameList = null;
        }

        /// <summary>
        /// Returns a string that specifies the data type of this ClipboardObject instance
        /// </summary>
        /// <returns>a string that specifies the data type of this ClipboardObject instance</returns>
        public string GetDataType => DataType;
    }

    /// <summary>
    /// Defines a ClipboardObject that contains a list of animations
    /// </summary>
    public class AnimationListClipboardObject : IClipboardObject
    {
        /// <summary>
        /// The data type identifier for the AnimationListClipboardObject
        /// </summary>
        public static readonly string DataType = "AnimationList";

        /// <summary>
        /// List of animations to store into this ClipboardObject
        /// </summary>
        List<Animation> _animList;

        /// <summary>
        /// Gets the animations currently added to this FrameListClipboardObject instance
        /// </summary>
        public Animation[] Aniamtions => _animList.ToArray();

        /// <summary>
        /// Initializes a new instance of the FrameListClipboardObject 
        /// </summary>
        public AnimationListClipboardObject()
        {
            _animList = new List<Animation>();
        }

        /// <summary>
        /// Initializes a new AnimationListClipboardObject with a list of frames to 
        /// </summary>
        /// <param name="animList">A list of animations to initialize the internal animation list with</param>
        public AnimationListClipboardObject(List<Animation> animList)
        {
            _animList = new List<Animation>(animList);
        }

        /// <summary>
        /// Adds the given Animation to this AnimationListClipboardObject instance
        /// </summary>
        /// <param name="anim">The frame to add</param>
        public void AddAnimation(Animation anim)
        {
            _animList.Add(anim);
        }

        /// <summary>
        /// Clears this clipboard object
        /// </summary>
        public void Clear()
        {
            if (_animList != null)
            {
                foreach (Animation anim in _animList)
                {
                    anim.Dispose();
                }

                _animList.Clear();
                _animList = null;
            }
        }

        /// <summary>
        /// Returns a string that specifies the data type of this ClipboardObject instance
        /// </summary>
        /// <returns>a string that specifies the data type of this ClipboardObject instance</returns>
        public string GetDataType => DataType;
    }

    /// <summary>
    /// Defines a image stream clipboard object
    /// </summary>
    public class ImageStreamClipboardObject : IClipboardObject
    {
        /// <summary>
        /// The data type identifier for the ImageStreamClipboardObject
        /// </summary>
        public static readonly string DataType = "ImageStream";

        /// <summary>
        /// The stream that contains the image data
        /// </summary>
        private readonly Stream _imageStream;

        /// <summary>
        /// Gets the stream that contains the image data
        /// </summary>
        public Stream ImageStream => _imageStream;

        /// <summary>
        /// Initializes a new instance of the ImageStreamClipboardObject class with a stream to initialzie the stream with
        /// </summary>
        /// <param name="stream">The stream containing the image data</param>
        public ImageStreamClipboardObject(Stream stream)
        {
            _imageStream = stream;
        }

        /// <summary>
        /// Clears this clipboard object
        /// </summary>
        public void Clear()
        {
            _imageStream.Dispose();
        }

        /// <summary>
        /// Returns a string that specifies the data type of this ClipboardObject instance
        /// </summary>
        /// <returns>a string that specifies the data type of this ClipboardObject instance</returns>
        public string GetDataType => DataType;
    }
}