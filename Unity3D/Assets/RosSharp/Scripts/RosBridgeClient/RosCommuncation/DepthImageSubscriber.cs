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
using System.Diagnostics;

namespace RosSharp.RosBridgeClient
{
    [RequireComponent(typeof(RosConnector))]
    public class DepthImageSubscriber : Subscriber<Messages.Sensor.CompressedImage>
    {
        public MeshRenderer meshRenderer;

        public int height;
        public int width;

        private Texture2D texture2D;
        private byte[] imageData;
        public byte[] ImageData
        {
            get { return imageData; }
        }

        private bool isMessageReceived;
        public bool IsMessageReceived
        {
            get { return isMessageReceived; }
        }
        
        private Messages.Standard.Time stamp;
        public Messages.Standard.Time Stamp
        {
            get { return stamp; }
        }

        protected override void Start()
        {
            base.Start();
            texture2D = new Texture2D(1, 1);
            meshRenderer.material = new Material(Shader.Find("Standard"));
        }
        private void Update()
        {
            if (isMessageReceived)
                ProcessMessage();
        }

        protected override void ReceiveMessage(Messages.Sensor.CompressedImage compressedImage)
        {
            stamp = compressedImage.header.stamp;
            imageData = compressedImage.data;
            isMessageReceived = true;
        }

        private void ProcessMessage()
        {
            texture2D.LoadImage(imageData);
            texture2D.Apply();
            meshRenderer.material.SetTexture("_MainTex", texture2D);
            isMessageReceived = false;
        }
    }
}

