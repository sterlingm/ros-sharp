﻿/*
© Siemens AG, 2017-2018
Author: Dr. Martin Bischoff (martin.bischoff@siemens.com)

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at
<http://www.apache.org/licenses/LICENSE-2.0>.
Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    [RequireComponent(typeof(RosConnector))]
    public class ImageRawSubscriber : Subscriber<Messages.Sensor.Image>
    {

        public int height;
        public int width;
        public string encoding;

        private byte[] imageData;
        public byte[] ImageData
        {
            get { return imageData; }
        }
        private bool isMessageReceived;

        private Messages.Standard.Time stamp;
        public Messages.Standard.Time Stamp
        {
            get { return stamp; }
        }
        protected override void Start()
        {
			base.Start();
            stamp = new Messages.Standard.Time();
        }
        private void Update()
        {
            if (isMessageReceived)
                ProcessMessage();
        }

        protected override void ReceiveMessage(Messages.Sensor.Image compressedImage)
        {
            double lastTime = (double)stamp.secs + (double)(stamp.nsecs * .000000001);
            double nowTime = (double)compressedImage.header.stamp.secs + (double)(compressedImage.header.stamp.nsecs * .000000001);
            //MonoBehaviour.print(string.Format("raw last time: {0} now time: {1}", lastTime, nowTime));
            MonoBehaviour.print(string.Format("raw elapsed time: {0}", (nowTime - lastTime)));
            stamp = compressedImage.header.stamp;
            imageData = compressedImage.data;
            isMessageReceived = true;
        }

        private void ProcessMessage()
        {
            isMessageReceived = false;
        }
    }
}

