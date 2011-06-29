// array to hold waypoint locations
var waypoints = new Array();

// variable to control time taken to travel between points
var duration : float = 1.0;

private var startPoint : Vector3;
private var endPoint : Vector3;
private var startTime : float;

private var wp : Transform;

// the array index number of the current target waypoint
private var targetwaypoint : int;

function Start() {
    startPoint = transform.position;
    startTime = Time.time;

    // store a reference to the transform of the waypoints object
    wp = GameObject.Find("waypoints").transform;

    // for each of the child objects of the waypoints object
    for (var child : Transform in wp) {

        // add them to the waypoints array
        waypoints.Push(child);
    }   


    if(waypoints.length <= 0){
        Debug.Log("No waypoints found");
        enabled = false;
    }

    targetwaypoint = 0;

    // set the initial end point to the first item in the array
    endPoint = waypoints[targetwaypoint].position;
    }

function Update () {

    var i = (Time.time - startTime) / duration;
    transform.position = Vector3.Lerp(startPoint, endPoint, i);

    // if we have reached the target
    if(i >= 1){

        // store the current time as the new start time
        startTime = Time.time;

        // store the value of the current item in the array and remove it from the array
        currentwaypoint = waypoints.Shift();
        // add the previously removed item to the end of the array
        waypoints.Add(currentwaypoint);


        // set our start point to our current position
        startPoint = endPoint;


        //set our new endpoint to the position of the new target waypoint
        endPoint = waypoints[targetwaypoint].position;

    }
}