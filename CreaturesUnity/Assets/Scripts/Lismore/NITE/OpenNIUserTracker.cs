using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using OpenNI;

public class OpenNIUserTracker : MonoBehaviour 
{
	private OpenNIContext Context;
	public int MaxCalibratedUsers;
    public float m_SmoothingFactor = 0.5f;

	public UserGenerator userGenerator;
	private SkeletonCapability skeletonCapbility;
	private PoseDetectionCapability poseDetectionCapability;
	private string calibPose;

	private List<int> allUsers;
	private List<int> calibratedUsers;
	private List<int> calibratingUsers;
	
	public IList<int> AllUsers
	{
		get { return allUsers.AsReadOnly(); }
	}
	public IList<int> CalibratedUsers
	{
		get { return calibratedUsers.AsReadOnly(); }
	}
	public IList<int> CalibratingUsers
	{
		get {return calibratingUsers.AsReadOnly(); }
	}
	
	public bool Mirror
	{
		get { return Context.Mirror; }
		set { Context.Mirror = value; }
	}
	
	bool AttemptToCalibrate
	{
		get { return calibratedUsers.Count < MaxCalibratedUsers; }
	}
	
	// Container for user information obtained via Kinect
    public struct UserInformation
    {
        public int id;
        public float initialVerticalDisplacement;
        public float lastVerticalDisplacement;
		public int creatureAssigned;
		public float lastHorizontalDisplacement;
		public float previousHorizontalDisplacement;

        public UserInformation(int userID)
        {
            id = userID;
            initialVerticalDisplacement = 0f;
            lastVerticalDisplacement = 0f;
			creatureAssigned = -1;
			lastHorizontalDisplacement = 0f;
			previousHorizontalDisplacement = 0f;
        }
    };
	
	public List<UserInformation> users;
	public int maxUsers = 3;
	
	// Flag to signal unassigned users
	public bool unassignedUser = false;
	
	// Counter to reduce rate at which we check for user position updates
	private int count = 0;
	
	void Start() 
	{
		// Make sure we have a valid OpenNIContext
		Context = OpenNIContext.Instance;
		if (null == Context)
		{
			print("OpenNI not inited");
			return;
		}
	
		calibratedUsers = new List<int>();
		calibratingUsers = new List<int>();
		allUsers = new List<int>();
		this.userGenerator = new UserGenerator(this.Context.context);
		this.skeletonCapbility = this.userGenerator.SkeletonCapability;
		this.poseDetectionCapability = this.userGenerator.PoseDetectionCapability;
		this.calibPose = this.skeletonCapbility.CalibrationPose;
		this.skeletonCapbility.SetSkeletonProfile(SkeletonProfile.All);

		// ncp Added
        this.skeletonCapbility.SetSmoothing(m_SmoothingFactor);
		
		
        this.userGenerator.NewUser += new EventHandler<NewUserEventArgs>(userGenerator_NewUser);
        this.userGenerator.LostUser += new EventHandler<UserLostEventArgs>(userGenerator_LostUser);
        this.poseDetectionCapability.PoseDetected += new EventHandler<PoseDetectedEventArgs>(poseDetectionCapability_PoseDetected);
        this.skeletonCapbility.CalibrationEnd += new EventHandler<CalibrationEndEventArgs>(skeletonCapbility_CalibrationEnd);
		
		unassignedUser = false;
		users = new List<UserInformation>();
	}

	void Update () 
	{
		if (Context.ValidContext)
		{
			// print("Update - UserTracker");
			Context.Update();
			UpdateUserInfo();
		}
	}
	
	void UpdateUserInfo()
	{			
		// Update all the users that we currently have registered
			// Only perform this update every 5 frames?!?
			if ( count == 1)
			{
			for(int i = 0; i < users.Count; i++)
			{
				var userInfo = users[i];
				var CoM = GetUserCenterOfMass(userInfo.id);
			
				// Check for not initialised yet...
				if(CoM.y != 0 )
				{			
					/*
					* Check vertical movement
					*/
					// Set initial reference height
					if (userInfo.initialVerticalDisplacement == 0)
					{
						userInfo.initialVerticalDisplacement = CoM.y;
					}
					// Ducking (500mms)
					if(CoM.y + 500 < userInfo.initialVerticalDisplacement)
					{
						//Debug.Log("User: " + userInfo.id + " is ducking");
                        userInfo.lastVerticalDisplacement = CoM.y;
					}
					// Jumping (500mms)
					if(CoM.y - 500 > userInfo.initialVerticalDisplacement)
					{
						//Debug.Log("User: " + userInfo.id + " is jumping");
                        userInfo.lastVerticalDisplacement = CoM.y;
					}
				
					/*
					* Check horizontal movement
					*/
				
					// Moved left or right greater than 500mm
					if(CoM.x  - 250 > userInfo.lastHorizontalDisplacement || CoM.x + 250 < userInfo.lastHorizontalDisplacement)
					{
						//Debug.Log("Last horizontal: " + userInfo.lastHorizontalDisplacement);
						//Debug.Log("CoM.X: " + CoM.X);
						// Check if we have moved close to previous (oscillating) within 100mm
						if (CoM.x - 100 < userInfo.previousHorizontalDisplacement && CoM.x + 100 > userInfo.previousHorizontalDisplacement)
						{
							// Go crazy!!!
							//Debug.Log("User: " + userInfo.id + " is going crazy");
						}
					
						userInfo.previousHorizontalDisplacement = userInfo.lastHorizontalDisplacement;
						userInfo.lastHorizontalDisplacement = CoM.x;
						//Debug.Log("User: " + userInfo.id + " has moved horizontally. Good for them.");
					}
				
					// Have to modify whole struct
					users[i] = userInfo;
				}
			}
			count = 0;
			}
			count++;
		
	}
	
	// Find a user by Kinect assigned ID
	public UserInformation GetUser(int userID)
	{
		foreach(UserInformation userInfo in users)
		{
			//Debug.Log("Tracking user:" + userInfo.id);
			if(userInfo.id == userID)
			{
				return userInfo;
			}
			
		}
		//Debug.Log("Cannot find user: " + userID);
		var emptyUser = new UserInformation(-1);
		return emptyUser;
	}
	
	#region Base OpenNIUserTracker 
	
    void skeletonCapbility_CalibrationEnd(object sender, CalibrationEndEventArgs e)
    {
        if (e.Success)
        {
            if (AttemptToCalibrate)
            {
                print("Starting tracking");
                this.skeletonCapbility.StartTracking(e.ID);
                calibratedUsers.Add(e.ID);
            }
        }
        else
        {
            if (AttemptToCalibrate)
            {
                this.poseDetectionCapability.StartPoseDetection(calibPose, e.ID);
            }
        }
		calibratingUsers.Remove(e.ID);
    }

    void poseDetectionCapability_PoseDetected(object sender, PoseDetectedEventArgs e)
    {
        print("Pose detected");
        this.poseDetectionCapability.StopPoseDetection(e.ID);
        if (AttemptToCalibrate)
        {
            print("Starting calibration");
            this.skeletonCapbility.RequestCalibration(e.ID, true);
			calibratingUsers.Add(e.ID);
        }
    }

    void userGenerator_LostUser(object sender, UserLostEventArgs e)
    {
        allUsers.Remove(e.ID);
        if (calibratedUsers.Contains(e.ID))
        {
            calibratedUsers.Remove(e.ID);
        }
		if (calibratingUsers.Contains(e.ID))
		{
			calibratingUsers.Remove(e.ID);
		}

        if (AttemptToCalibrate)
        {
            AttemptCalibrationForAllUsers();
        }
		
		 foreach(UserInformation userInfo in users)
				{
					if (userInfo.id == e.ID)
					{
						users.Remove(userInfo);
						break;
					}
				}
    }

    void userGenerator_NewUser(object sender, NewUserEventArgs e)
    {
        allUsers.Add(e.ID);
        if (AttemptToCalibrate)
        {
            print("Starting pose detection");
            this.poseDetectionCapability.StartPoseDetection(this.calibPose, e.ID);
        }
		
		// Check array size against max-users
		if (users.Count < maxUsers)
		{
				var newUser = new UserInformation(e.ID);
				users.Add(newUser);
				unassignedUser = true;
			
		}
    }
	
	void AttemptCalibrationForAllUsers()
	{
		foreach (int id in userGenerator.GetUsers())
		{
			if (!skeletonCapbility.IsCalibrating(id) && !skeletonCapbility.IsTracking(id))
			{
				this.poseDetectionCapability.StartPoseDetection(this.calibPose, id);
			}
		}
	}
	
	public void UpdateSkeleton(int userId, OpenNISkeleton skeleton)
	{
		// make sure we have skeleton data for this user
		if (!skeletonCapbility.IsTracking(userId))
		{
			return;
		}
		
		// Use torso as root
		SkeletonJointTransformation skelTrans = new SkeletonJointTransformation();
		skelTrans = skeletonCapbility.GetSkeletonJoint(userId, SkeletonJoint.Torso);
		skeleton.UpdateRoot(skelTrans.Position.Position);
		
		// update each joint with data from OpenNI
		foreach (SkeletonJoint joint in Enum.GetValues(typeof(SkeletonJoint)))
		{
			if (skeletonCapbility.IsJointAvailable(joint))
			{
				skelTrans = skeletonCapbility.GetSkeletonJoint(userId, joint);
				skeleton.UpdateJoint(joint, skelTrans);
			}
		}
	}
	
	public Vector3 GetUserCenterOfMass(int userId)
	{
		Point3D com = userGenerator.GetCoM(userId);
		return new Vector3(com.X, com.Y, -com.Z);
	}
	
	#endregion
	
}
