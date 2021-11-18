HomingHead HH; 
IMyTextPanel TP1, TP2; 
IMyTimerBlock Timer; 
int Tick=0; 
bool RadarActive; 
 
Torpedo Torp1; //, Torp2; 
 
 
void Main(string argument) 
{ 
	Tick++; 
// создаем необходимые объекты, в т.ч. объект нашего собственного класса HomingHead. 
	if (TP1==null) 
		TP1 = GridTerminalSystem.GetBlockWithName("TP1") as IMyTextPanel;  
	if (TP2==null) 
		TP2 = GridTerminalSystem.GetBlockWithName("TP2") as IMyTextPanel;  
	if (HH==null) 
		HH = new HomingHead(this);	 
	if (Timer==null)	 
		Timer = GridTerminalSystem.GetBlockWithName("Timer") as IMyTimerBlock; 
	 
	if (Torp1==null) 
		Torp1 = new Torpedo(this, "Torpedo1");		 
	//if (Torp2==null) 
		//Torp2 = new Torpedo(this, "Torpedo2");		 
	 
// разбираем аргументы, с которыми скрипт был запущен	 
	if (argument=="TryLock") 
		{ 
		HH.Lock(true, 15000); 
		if (HH.CurrentTarget.EntityId!=0) 
			RadarActive=true; 
		else 
			RadarActive=false; 
		}		 
	else if (argument=="Stop") 
		{ 
		Timer.GetActionWithName("Stop").Apply(Timer); 
		HH.StopLock(); 
		RadarActive=false; 
		} 
	if (argument=="Launch") 
		{ 
			Torp1.Launch(); 
//			Torp2.Launch(); 
		}		 
	else 
		{ 
			HH.Update(); 
			Torp1.Update();			 
			//Torp2.Update(); 
		}		 
// если в захвате находится какой-то объект, то выполнение скрипта зацикливается		 
	if (RadarActive) 
		Timer.GetActionWithName("TriggerNow").Apply(Timer); 
} 
 
IMyTerminalBlock GetBlockByName(string blockName) 
{ 
    // First define the variable to store the found block 
    var blocks = new List<IMyTerminalBlock>(); 
    // start the search and store back into the variable 
    GridTerminalSystem.SearchBlocksOfName(blockName, blocks); 
    // return the found block 
	if (blocks.Count>0) 
		return blocks[0]; 
	else 
		return null; 
} 
 
public class Torpedo    
{  
	Program ParentProgram;   
	public string Prefix;  	  
	public Vector3D MyPos;  
	public Vector3D MyPrevPos;  
	public Vector3D MyVelocity;  
	public Vector3D InterceptVector;  
	public double TargetDistance;  
	public int Status; //0-destroyed or doesn't exist, 1-ready to launch, 2-launched.  
	public int LaunchDelay=300; 
	private IMyGyro TorpGyro;  
	private IMyThrust TorpThrust;  
	//private IMyWarhead TorpWarhead;  
	private IMyTerminalBlock TorpMerge;  
	 
	private List<IMyTerminalBlock> Decouplers;  
	  
	public Torpedo(Program MyProg, string TorpedoPrefix)  
	{    
		ParentProgram = MyProg;  
		Prefix = TorpedoPrefix;  
		TorpGyro = ParentProgram.GridTerminalSystem.GetBlockWithName(Prefix + "Gyro") as IMyGyro;  
		TorpThrust = ParentProgram.GetBlockByName(Prefix + "Thruster") as IMyThrust;  
		//TorpWarhead = ParentProgram.GridTerminalSystem.GetBlockWithName(Prefix + "Warhead") as IMyWarhead;  
		TorpMerge = ParentProgram.GridTerminalSystem.GetBlockWithName(Prefix + "Merge") as IMyTerminalBlock;  
		Decouplers = new List<IMyTerminalBlock>(); 
		ParentProgram.GridTerminalSystem.SearchBlocksOfName(Prefix+"Decoupler", Decouplers); 
		 
		if ((TorpGyro!=null)&&(TorpThrust!=null)&&(TorpMerge!=null))  
		{  
			Status = 1;  
		}  
		else  
		{  
			Status = 0;  
		}  
	}	  
     
	public void Decouple() 
	{ 
		for (int i = 0;i < Decouplers.Count;i++)        
		{        
			IMyTerminalBlock Decoupler = Decouplers[i] as IMyTerminalBlock;        
			if (Decoupler != null)        
			{        
				Decoupler.ApplyAction("OnOff_Off");        
			}        
		}        
	} 
	 
	public void Update()  
	{    
		if ((TorpGyro==null)&&(TorpThrust==null))  
			Status=0;  
		if (Status == 2)  
		{  
			LaunchDelay--; 
			MyPos = TorpGyro.GetPosition();  
			MyVelocity = (MyPos - MyPrevPos)*60;  
			MyPrevPos = MyPos;  
			TargetDistance=(ParentProgram.HH.correctedTargetLocation-MyPos).Length(); 
			if (TargetDistance<200) 
				Decouple(); 
			InterceptVector = FindInterceptVector(MyPos, MyVelocity.Length(), ParentProgram.HH.correctedTargetLocation, ParentProgram.HH.CurrentTarget.Velocity);  
			if (LaunchDelay<0) 
				SetGyroOverride(GetNavAngles(InterceptVector));  
 
			//if ((ParentProgram.HH.correctedTargetLocation-MyPos).Length()<150)  
				//TorpWarhead.ApplyAction("Detonate");  
		}  
		UpdateTorpedoInfo();  
	}  
	  
	public void UpdateTorpedoInfo()  
	{  
		ParentProgram.TP1.WritePublicText("\n Torpedo Status:" + Status.ToString()+ " \n", true);  
	}  
  
	public void SetGyroOverride(Vector3D settings)  
	{  
        if (TorpGyro != null)         
        {     
			if (!TorpGyro.GyroOverride)  
				TorpGyro.ApplyAction("Override");         
            TorpGyro.Yaw=(float)settings.GetDim(0);         
            TorpGyro.Pitch=-(float)settings.GetDim(1);         
            TorpGyro.Roll=(float)settings.GetDim(2)/5;         
        }         
	}  
 	  
	public void Launch()  
	{    
		if ((Status==1)&&(ParentProgram.HH.CurrentTarget.EntityId!=0))  
		{  
			TorpThrust.GetActionWithName("OnOff_On").Apply(TorpThrust);  
			TorpThrust.ThrustOverridePercentage=100f;  
			TorpMerge.GetActionWithName("OnOff_Off").Apply(TorpMerge);  
			Status=2;  
		}  
		UpdateTorpedoInfo();  
	}  
	  
	  
	private Vector3D GetNavAngles(Vector3D Target)         
	{         
       
		Vector3D V3Dfow = TorpGyro.WorldMatrix.Forward;         
		Vector3D V3Dup = TorpGyro.WorldMatrix.Up;         
		Vector3D V3Dleft = TorpGyro.WorldMatrix.Left;         
	     
		Vector3D TargetNorm = Vector3D.Normalize(Target);     
		Vector3D VectorReject = Vector3D.Reject(Vector3D.Normalize(MyVelocity),TargetNorm);	  
	    Vector3D CorrectionVector = Vector3D.Normalize(TargetNorm-VectorReject*2);  
	     
		double TargetPitch = Vector3D.Dot(V3Dup, Vector3D.Normalize(Vector3D.Reject(CorrectionVector,V3Dleft)));     
		TargetPitch =Math.Acos(TargetPitch)-Math.PI/2;   
		double TargetYaw = Vector3D.Dot(V3Dleft, Vector3D.Normalize(Vector3D.Reject(CorrectionVector,V3Dup)));         
		TargetYaw =Math.Acos(TargetYaw)-Math.PI/2;   
		double RollMult=Math.Abs(TargetYaw)+Math.Abs(TargetPitch); 
		double TargetRoll = Math.Min(1/RollMult,30); 
			if (RollMult>0.3f) 
				TargetRoll=0;   
	     
		return new Vector3D(TargetYaw, -TargetPitch, TargetRoll);         
	}    
 
	private Vector3D FindInterceptVector(Vector3D shotOrigin, double shotSpeed,  
		Vector3D targetOrigin, Vector3D targetVel)  
	{  
		Vector3D dirToTarget = Vector3D.Normalize(targetOrigin - shotOrigin);  
		Vector3D targetVelOrth = Vector3D.Dot(targetVel, dirToTarget) * dirToTarget;  
		Vector3D targetVelTang = targetVel - targetVelOrth;  
		Vector3D shotVelTang = targetVelTang;  
		double shotVelSpeed = shotVelTang.Length();  
		  
		if (shotVelSpeed > shotSpeed) {  
			return Vector3D.Normalize(targetVel) * shotSpeed;  
		} else {  
			double shotSpeedOrth = Math.Sqrt(shotSpeed * shotSpeed - shotVelSpeed * shotVelSpeed);  
			Vector3D shotVelOrth = dirToTarget * shotSpeedOrth;  
			return shotVelOrth + shotVelTang;  
		}  
	}	 
} 
 
 
public class HomingHead //Наш собственный класс для захвата цели с помощью массива камер.  
{ 
	Program ParentProgram; //Ссылка на программу, "породившую объект этого класса" (на наш скрипт т.е.) 
	private static string Prefix = "Camera";  //префикс камер (с этого слова начинаются названия всех камер)  
	private List<IMyTerminalBlock> CamArray; //массив камер 
	private int CamIndex; //индекс текущей камеры в массиве 
	public MyDetectedEntityInfo CurrentTarget; // структура инфы о захваченном объекте 
	public Vector3D MyPos; // координаты 1й камеры (они и будут считаться нашим положением) 
	public Vector3D correctedTargetLocation; //расчетные координаты захваченного объекта. (прежние координаты+вектор скорости * прошедшее время с последнего обновления захвата) 
	public double TargetDistance; //расстояние до ведомой цели	 
	public int LastLockTick; // программный тик последнего обновления захвата 
	public int TicksPassed; // сколько тиков прошло с последнего обновления захвата 
	 
	 
	// это конструктор. Он выполняется при создании объекта этого класса. Здесь я инициализирую массив камер, которые будут участвовать в захвате и сопровождении цели. 
	public HomingHead(Program MyProg)  
	{   
		ParentProgram = MyProg;  
		CamIndex = 0; 
		CamArray = new List<IMyTerminalBlock>(); 
		ParentProgram.GridTerminalSystem.SearchBlocksOfName(Prefix, CamArray);   
		ParentProgram.TP1.WritePublicText("", false); 
		for (int i = 0; i < CamArray.Count; i++)   
		{   
			IMyCameraBlock Cam = CamArray[i] as IMyCameraBlock;   
			if (Cam != null)   
			{   
				Cam.EnableRaycast = true; 
				//ParentProgram.TP1.WritePublicText(" " + Cam.CustomName + " - ", true); 
			}   
		}   
	}   
	 
	//Это основной метод, который осуществляет первоначальный захват и дальнейшее сопровождение цели с помощью массива камер.  
	public void Lock(bool TryLock=false, double InitialRange=10000)  
	{ 
		int initCamIndex=CamIndex; 
		MyDetectedEntityInfo lastDetectedInfo = CurrentTarget; 
		bool CanScan=true; 
		// найдем первую после использованной в последний раз камеру, которая способна кастануть лучик на заданную дистанцию. 
		if (CurrentTarget.EntityId == 0) 
			TargetDistance=InitialRange; 
		 
		while ((CamArray[CamIndex] as IMyCameraBlock)?.CanScan(TargetDistance)==false) 
		{ 
			CamIndex++; 
			if (CamIndex>=CamArray.Count) 
				CamIndex=0; 
			if (CamIndex==initCamIndex) 
			{ 
				CanScan=false; 
				break; 
			} 
		} 
		//если такая камера в массиве найдена - кастуем ей луч. 
		if (CanScan) 
		{ 
			//в случае, если мы осуществляем первоначальный захват цели, кастуем луч вперед 
			if ((TryLock)&&(CurrentTarget.EntityId == 0)) 
				lastDetectedInfo = (CamArray[CamIndex] as IMyCameraBlock).Raycast(InitialRange, 0, 0); 
			else //иначе - до координат предполагаемого нахождения цели.	 
				lastDetectedInfo = (CamArray[CamIndex] as IMyCameraBlock).Raycast(correctedTargetLocation); 
			//если что-то нашли лучем, то захват обновлен	 
			if (lastDetectedInfo.EntityId != 0) 
			{ 
				CurrentTarget = lastDetectedInfo; 
				LastLockTick = ParentProgram.Tick; 
				TicksPassed = 0; 
			} 
			else //иначе - захват потерян 
			{ 
				ParentProgram.TP1.WritePublicText("Target Lost" + " \n", false); 
				CurrentTarget=lastDetectedInfo; 
			}	 
			CamIndex++; //перебираем камеры в массиве по-очереди. 
			if (CamIndex>=CamArray.Count) 
				CamIndex=0;			 
		} 
	}	 
	 
	//этот метод сбрасывает захват цели 
	public void StopLock() 
	{ 
		CurrentTarget = (CamArray[0] as IMyCameraBlock).Raycast(0, 0, 0);	 
	} 
	 
	// этот метод выводит данные по захваченному объекту на панель 
	public void TargetInfoOutput() 
	{ 
		if (CurrentTarget.EntityId!=0) 
		{		 
			ParentProgram.TP2.WritePublicText("Target Info:" + " \n", false); 
			ParentProgram.TP2.WritePublicText(CurrentTarget.EntityId + " \n", true);	 
			ParentProgram.TP2.WritePublicText(CurrentTarget.Type + " \n", true); 
			ParentProgram.TP2.WritePublicText(CurrentTarget.Name + " \n", true); 
			ParentProgram.TP2.WritePublicText("Position:" + " \n", true); 
			ParentProgram.TP2.WritePublicText("Size: " + CurrentTarget.BoundingBox.Size.ToString("0.0") + " \n", true);			 
			ParentProgram.TP2.WritePublicText("X: " + Math.Round(CurrentTarget.Position.GetDim(0),2).ToString() + " \n", true); 
			ParentProgram.TP2.WritePublicText("Y: " + Math.Round(CurrentTarget.Position.GetDim(1),2).ToString() + " \n", true); 
			ParentProgram.TP2.WritePublicText("Z: " + Math.Round(CurrentTarget.Position.GetDim(2),2).ToString() + " \n", true); 
			ParentProgram.TP2.WritePublicText("Velocity: " + Math.Round(CurrentTarget.Velocity.Length(),2).ToString() + " \n", true);			 
			ParentProgram.TP2.WritePublicText("Distance: " + Math.Round(TargetDistance,2).ToString() + " \n", true);	 
			}	 
		else 
			ParentProgram.TP2.WritePublicText("NO TARGET" + " \n", false);	 
	} 
	 
	 
	//этот метод выполняет общее обновление объекта.  
	public void Update() 
	{ 
		MyPos=CamArray[0].GetPosition(); 
		//если в захвате находится какой-то объект, выполняем следующие действия 
		if (CurrentTarget.EntityId!=0) 
		{ 
			TicksPassed = ParentProgram.Tick-LastLockTick; 
			//считаем предполагаемые координаты цели (прежние координаты + вектор скорости * прошедшее время с последнего обновления захвата) 
			correctedTargetLocation = CurrentTarget.Position + (CurrentTarget.Velocity*TicksPassed/60); 
			// добавим к дистанции до объекта 10 м (так просто для надежности) 
			TargetDistance=(correctedTargetLocation-MyPos).Length()+10; 
			 
			//дальнейшее выполняется в случае, если пришло время обновить захват цели. Частота захвата в тиках считается как дистанция до объекта / 2000 * 60 / кол-во камер в массиве 
			// 2000 - это скорость восстановления дальности raycast по умолчанию) 
			// на 60 умножаем т.к. 2000 восстанавливается в сек, а в 1 сек 60 программных тиков 
			if (TicksPassed>TargetDistance*0.03/CamArray.Count) 
			{ 
				Lock(); 
				 
				ParentProgram.TP1.WritePublicText("Cam array info:" + " \n", false); 
				ParentProgram.TP1.WritePublicText("Cam quantity:" + CamArray.Count.ToString() + " \n", false);				 
				ParentProgram.TP1.WritePublicText("Cam: " +CamArray[CamIndex].CustomName + " \n", true); 
				ParentProgram.TP1.WritePublicText("Distance: " +  TargetDistance.ToString() + " \n", true);				 
				ParentProgram.TP1.WritePublicText("Delay: " +  Math.Round(TargetDistance*0.03/CamArray.Count,0).ToString() + " \n", true); 
				 
				TargetInfoOutput(); 
			}	 
		}	 
	} 
}	 
 
