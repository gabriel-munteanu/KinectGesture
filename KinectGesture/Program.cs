using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using WindowsInput;

namespace KinectGesture
{
    class Program
    {
        static KinectSensor _kinectSensor = null;
        static BodyFrameReader _bodyFrameReader = null;
        static Body[] _bodies = null;
        static bool _gestureRecognised = false;
        static DateTime _lastGestureTimestamp = DateTime.Now;
        static bool _endGestureRecognised = false;

        static void Main(string[] args)
        {

            _kinectSensor = KinectSensor.GetDefault();
            _bodyFrameReader = _kinectSensor.BodyFrameSource.OpenReader();
            _kinectSensor.Open();
            _bodyFrameReader.FrameArrived += bodyFrameReader_FrameArrived;
            while (!_endGestureRecognised) ;
            _kinectSensor.Close();
        }

        static void bodyFrameReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;

            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (_bodies == null)
                    {
                        _bodies = new Body[bodyFrame.BodyCount];
                    }

                    // The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
                    // As long as those body objects are not disposed and not set to null in the array,
                    // those body objects will be re-used.
                    bodyFrame.GetAndRefreshBodyData(_bodies);
                    dataReceived = true;
                }

                if (!dataReceived)
                    return;

                foreach (var body in _bodies)
                    if (body.IsTracked)
                        CheckForGesture(body);
            }


        }


        private static void CheckForGesture(Body body)
        {
            if ((DateTime.Now - _lastGestureTimestamp).Seconds < 1)
                return;
            float d = d = JointDistance(body.Joints[JointType.HandRight], body.Joints[JointType.Head]);
            if (GestureMeetConditions(d))
            {
                Console.WriteLine("Esc");
                //_endGestureRecognised = true;
                InputSimulator.SimulateKeyPress(VirtualKeyCode.ESCAPE);
                return;
            }

            d = JointDistance(body.Joints[JointType.HandRight], body.Joints[JointType.ShoulderLeft]);
            if (GestureMeetConditions(d))
            {
                Console.WriteLine("Next");
                InputSimulator.SimulateKeyPress(VirtualKeyCode.RIGHT);
                return;
            }

            d = JointDistance(body.Joints[JointType.HandLeft], body.Joints[JointType.ShoulderRight]);
            if (GestureMeetConditions(d))
            {
                Console.WriteLine("Previous");
                InputSimulator.SimulateKeyPress(VirtualKeyCode.LEFT);
                return;
            }

            d = JointDistance(body.Joints[JointType.HandLeft], body.Joints[JointType.Head]);
            if (GestureMeetConditions(d))
            {
                Console.WriteLine("Start from current");
                InputSimulator.SimulateKeyDown(VirtualKeyCode.SHIFT);
                InputSimulator.SimulateKeyPress(VirtualKeyCode.F5);
                InputSimulator.SimulateKeyUp(VirtualKeyCode.SHIFT);
                return;
            }

            _gestureRecognised = false;
        }

        private static bool GestureMeetConditions(float d)
        {
            if (d < 0.2f && _gestureRecognised == false)
            {
                _gestureRecognised = true;
                _lastGestureTimestamp = DateTime.Now;
                return true;
            }
            return false;
        }

        private static float JointDistance(Joint first, Joint second)
        {
            float dx = first.Position.X - second.Position.X;
            float dy = first.Position.Y - second.Position.Y;
            float dz = first.Position.Z - second.Position.Z;
            return (float)Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
        }
    }
}
