using UnityEngine;
using RosSharp.RosBridgeClient;
using System.Runtime.InteropServices;
using System.Diagnostics;

public class RGBDMerger : MonoBehaviour
{

    [DllImport("image_decompress_opencv.dll", EntryPoint = "processImage")]
    public extern static void processImage(byte[] raw, int lenCompressed, int flag, int width, int height, int len, out System.IntPtr image);


    public ImageSubscriber rgbImageSub;
    public DepthImageSubscriber depthImageSub;
    public ImageRawSubscriber depthImageRawSub;


    public RosSharp.RosBridgeClient.Messages.Sensor.Image rgbImage;
    public RosSharp.RosBridgeClient.Messages.Sensor.Image depthImage;

    public bool usingCompressedDepth;

    public string TopicRGB;
    private string publicationIdRGB;
    public string TopicDepth;
    private string publicationIdDepth;


    // Start is called before the first frame update
    void Start()
    {
        publicationIdRGB = GetComponent<RosConnector>().RosSocket.Advertise<RosSharp.RosBridgeClient.Messages.Sensor.Image>(TopicRGB);
        publicationIdDepth = GetComponent<RosConnector>().RosSocket.Advertise<RosSharp.RosBridgeClient.Messages.Sensor.Image>(TopicDepth);
        rgbImage = new RosSharp.RosBridgeClient.Messages.Sensor.Image();
        depthImage = new RosSharp.RosBridgeClient.Messages.Sensor.Image();
    }

    private void OnDestroy()
    {
    }

    // Update is called once per frame
    void Update()
    {
        RosSharp.RosBridgeClient.Messages.Standard.Time rgbStamp = null;
        RosSharp.RosBridgeClient.Messages.Standard.Time depthStamp = null;

        // If we are getting a compressed depth image
        if (usingCompressedDepth)
        {
            // Make sure both images have been received
            if (rgbImageSub.ImageData != null && depthImageSub.ImageData != null)
            {
                rgbStamp = rgbImageSub.Stamp;
                depthStamp = depthImageSub.Stamp;
            }
        }
        // Else, we're getting the raw depth image
        else
        {
            // Make sure both images have been received
            if (rgbImageSub.ImageData != null && depthImageRawSub.ImageData != null)
            {
                rgbStamp = rgbImageSub.Stamp;
                depthStamp = depthImageRawSub.Stamp;
            }
        }


        // Check if the images are stamped close together
        if (rgbStamp != null && (rgbStamp.secs - depthStamp.secs < 1))
        {
            DecompressImages();
            if (usingCompressedDepth == false)
            {
                depthImage.data = depthImageRawSub.ImageData;
            }

            // Test by publishing image and checking rviz
            PublishRGB();
            PublishDepth();
            //MergeImages();
        }
    }


    protected void PublishRGB()
    {
        setRGBMsgFields();
        GetComponent<RosConnector>().RosSocket.Publish(publicationIdRGB, rgbImage);
    }

    protected void PublishDepth()
    {
        setDepthMsgFields();
        GetComponent<RosConnector>().RosSocket.Publish(publicationIdDepth, depthImage);
    }

    // For publishing for testing
    private void setRGBMsgFields()
    {
        // Set image fields
        rgbImage.encoding = rgbImageSub.encoding;
        rgbImage.height = 480;
        rgbImage.width = 640;
        rgbImage.is_bigendian = 0;
        rgbImage.step = 1920;

        rgbImage.header.frame_id = "camera_rgb_optical_frame";
    }
    

    // For publishing for testing
    private void setDepthMsgFields()
    {
        // Set image fields for png
        depthImage.encoding = usingCompressedDepth ? depthImageSub.encoding : depthImageRawSub.encoding;
        depthImage.height = 480;
        depthImage.width = 640;
        depthImage.is_bigendian = 0;
        depthImage.step = 1280;
        depthImage.header.frame_id = "camera_rgb_optical_frame";
    }

    private void DecompressRGB()
    {
        // Calculate number of elements in byte array
        int len= rgbImageSub.width * rgbImageSub.height;
        if (rgbImageSub.encoding.Equals("rgb8"))
        {
            len *= 3;
        }
        // Put in other encoding representations...

        // Allocate managed memory array
        rgbImage.data = new byte[len];

        // Allocate unmanaged memory
        System.IntPtr mem = Marshal.AllocHGlobal(len);

        // Call dllimport function to fill in unmanaged memory
        processImage(rgbImageSub.ImageData, rgbImageSub.ImageData.Length, 0, rgbImageSub.width, rgbImageSub.height, len, out mem);

        //Debug.Log(string.Format("rgbImage data size: {0} mem size: {1} rgb.data size: {2}", rgbImageSub.ImageData.Length, mem.GetType(), rgbImage.data.Length));

        // Copy unmanaged memory into managed byte array
        Marshal.Copy(mem, rgbImage.data, 0, len);

        // Deallocate unmanaged memory
        // Unity crashes here?
        Marshal.FreeHGlobal(mem);
    }

    private void DecompressDepth()
    {
        // Calculate number of elements in byte array
        // Dynamically determine number of bits to represent pixels? 8bit vs 16bit vs 32bit
        // pngs are 32 bit so *4 is used
        int lenDepth = (depthImageSub.width * depthImageSub.height);
        if (depthImageSub.encoding.Equals("32FC1"))
        {
            lenDepth *= 4;
        }
        // Put in other encoding representations...

        // Allocate managed memory array
        depthImage.data = new byte[lenDepth];

        // Allocate unmanaged memory
        System.IntPtr memDepth = Marshal.AllocHGlobal(lenDepth);

        // Call dllimport function to fill in unmanaged memory
        processImage(depthImageSub.ImageData, depthImageSub.ImageData.Length, 1, depthImageSub.width, depthImageSub.height, lenDepth, out memDepth);

        // Copy unmanaged memory into managed byte array
        Marshal.Copy(memDepth, depthImage.data, 0, lenDepth);

        // Deallocate unmanaged memory
        // Unity crashes here?
        Marshal.FreeHGlobal(memDepth);
    }

    void DecompressImages()
    {
        // Each decompression takes ~10-30ms

        // Always using RGB compressed
        DecompressRGB();

        // Check if using depth compressed
        if (usingCompressedDepth)
        {
            DecompressDepth();
        }
    }

}