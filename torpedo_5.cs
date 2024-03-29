int _runcount = 0;
string _broadCastTag = "Torpedo";
IMyBroadcastListener _myBroadcastListener;

public Program()
{
    Echo("Creator");
    _myBroadcastListener=IGC.RegisterBroadcastListener(_broadCastTag);
    _myBroadcastListener.SetMessageCallback(_broadCastTag); 
}

public void Save()
{
    Vector3D target;
}

public void Main(string argument, UpdateType updateSource)
        {
            float Yaw = 0;
            float Pitch = 0;
            float Roll = 0;
            var Gyro = GridTerminalSystem.GetBlockWithName("Gyro") as IMyGyro;
            Gyro.ApplyAction("Override");
            MyDetectedEntityInfo lastDetectedInfo;
            double InitialRange = 1000;
            IMyCameraBlock Cam = GridTerminalSystem.GetBlockWithName("Cam") as IMyCameraBlock;
            Cam.EnableRaycast = true;
            argument = System.DateTime.Now.ToString();
            if (Cam.CanScan(InitialRange))
            {                
                lastDetectedInfo = Cam.Raycast(InitialRange);
                //Cam.RaycastConeLimit = 45;
                
                //gyro                
                //Echo("Calling GetNavAngles");
                Vector3D settings = GetNavAngles(lastDetectedInfo.Position, Gyro);
                Yaw = (float)settings.GetDim(0);
                Pitch = -(float)settings.GetDim(1);
                Roll = (float)settings.GetDim(2) / 5;

                //Gyro override
                                
                Gyro.Yaw = 0;//Yaw;           
                Gyro.Pitch = 0;//Pitch;
                Gyro.Roll = 0;//Roll;

                Echo("Type: " + lastDetectedInfo.Type.ToString());
                Echo("Name: " + lastDetectedInfo.Name);
                //Echo("Orientation: "+lastDetectedInfo.Orientation.ToString());
                //Echo("Velocity: " + lastDetectedInfo.Velocity.ToString());
                //Echo("Position: " + lastDetectedInfo.Position.ToString());
                argument += "\nName: " + lastDetectedInfo.Name;
                //argument += "\nVelocity: " + lastDetectedInfo.Velocity.ToString();
                argument += "\nPosition X: " + lastDetectedInfo.Position.X.ToString();
                argument += "\nPosition Y: " + lastDetectedInfo.Position.Y.ToString();
                argument += "\nPosition Z: " + lastDetectedInfo.Position.Z.ToString();
                //argument += "\nYaw: " + Yaw.ToString();
                //argument += "\nPitch: " + Pitch.ToString();
                //argument += "\nRoll: " + Roll.ToString();
                //argument += "\nGyro.Orientation: " + Gyro.Orientation.Forward.ToString();                
                //argument += "\nGyroUp.X: " + Gyro.WorldMatrix.Up.X.ToString();
                //argument += "\nGyroUp.Y: " + Gyro.WorldMatrix.Up.Y.ToString();
                //argument += "\nGyroUp.Z: " + Gyro.WorldMatrix.Up.Z.ToString();
            }
            else 
            {
                Echo("raycast unavailable");
                Gyro.Yaw = 0;//Yaw;           
                Gyro.Pitch = 0;//Pitch;
                Gyro.Roll = 0;//Roll;
            }
            //TorpThrust = GetBlockByName("Thruster") as IMyThrust;

            // Antenna cast
            _runcount++;
            Echo(_runcount.ToString() + ":" + updateSource.ToString());
            if (
                (updateSource & (UpdateType.Trigger | UpdateType.Terminal)) > 0
                || (updateSource & (UpdateType.Mod)) > 0
                || (updateSource & (UpdateType.Script)) > 0
                )
            {
                if (argument != "")
                {
                    IGC.SendBroadcastMessage(_broadCastTag, argument);
                    Echo("Sending message:\n" + argument);
                }
            }           

            

            var Timer = GridTerminalSystem.GetBlockWithName("TorpedoTimer") as IMyTimerBlock;
            Timer.GetActionWithName("Start").Apply(Timer);
        }

       private Vector3D GetNavAngles(Vector3D Target, IMyGyro Gyro)   
{

Vector3D MyPos = Gyro.GetPosition();
Vector3D MyVelocity = MyPos-MyPos;

Vector3D V3Dfow = Gyro.WorldMatrix.Forward;
Vector3D V3Dup = Gyro.WorldMatrix.Up;
Vector3D V3Dleft = Gyro.WorldMatrix.Left;

Echo("V3Dup: "+V3Dup.ToString());

//управляем вектором тяги так, чтобы гасить боковую скорость.
Vector3D TargetNorm = Vector3D.Normalize(Target);
Vector3D VectorReject = Vector3D.Reject(Vector3D.Normalize(MyVelocity),TargetNorm);
Vector3D CorrectionVector = Vector3D.Normalize(TargetNorm-VectorReject*2);

double TargetPitch = Vector3D.Dot(V3Dup, Vector3D.Normalize(Vector3D.Reject(CorrectionVector,V3Dleft)));
TargetPitch =Math.Acos(TargetPitch)-Math.PI/2;

double TargetYaw = Vector3D.Dot(V3Dleft, Vector3D.Normalize(Vector3D.Reject(CorrectionVector,V3Dup)));
TargetYaw =Math.Acos(TargetYaw)-Math.PI/2;

double RollMult=Math.Abs(TargetYaw)+Math.Abs(TargetPitch);

//торпеда крутится вокруг продольной оси. Для лучшего разброса стальных кубиков или отстыкованных блоков.
double TargetRoll = Math.Min(1/RollMult,30);
if (RollMult>0.3f)
TargetRoll=0;
//TargetPitch = V3Dup;
return new Vector3D(TargetYaw, TargetPitch, TargetRoll);
}
