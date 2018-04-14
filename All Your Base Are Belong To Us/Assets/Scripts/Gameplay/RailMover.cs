﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpeedMode
{
    ConstantSpeed,
    TimeBetweenSegments,
}
public class RailMover : MonoBehaviour {

    public Rail rail;
    public PlayMode playMode;                   //Choose between Catmull-Rom or Linear splines for the path
    public SpeedMode speedMode;                 //Choose between  moving at constant speed or in a certain time
    public OrientationMode orientationMode;     //Choose between  orient the object on the rail with the nodes orientation as reference or looking at next node
    public float speed = 2.5f;                  //Speed to move (only if ConstantSpeed is selected on SpeedMode)
    public float timeBetweenSegments = 2.5f;    //Time to reach next node (only if TimeBetweenSegments is selected on SpeedMode)
    public bool isReversed;                     //If true it reverses the path
    public bool isLooping;                      //If true it loops the path when reaching the end node
    public bool pingPong;                       //If true when the end node is reached it reverses the path

    private int currentSeg;
    private float transition;
    private bool isCompleted;

    private void Update()
    {
        //There is not rail exit
        if (!rail)
            return;

        //Avoid using Catmull when less than 4 nodes exist
        if (rail.nodes.Length < 4 && playMode.ToString().Equals("Catmull"))
        {
            print("Not enought nodes for Catmull (minimun of 4 nodes is required, switching to Linear)");
            playMode = PlayMode.Linear;
        }

        //Play the animation if it is not completed
        if (!isCompleted)
            Play(!isReversed);
    }

    /// <summary>
    /// Plays the path following animation
    /// </summary>
    /// <param name="forward"> A boolean to make it go forward or backwards </param>
    private void Play(bool forward = true)
    {
        switch (speedMode)          //Choose between using speed or time
        {
            default:

            case SpeedMode.ConstantSpeed: //Make it to move at a constant speed on every node
                float m = (rail.nodes[currentSeg + 1].position - rail.nodes[currentSeg].position).magnitude;
                float s = (Time.deltaTime * 1 / m) * speed;
                transition += (forward) ? s : -s;
                break;

            case SpeedMode.TimeBetweenSegments: //Make the object to reach next node in timeBetweenSegments
                float move = Time.deltaTime * 1 / timeBetweenSegments;
                transition += (forward) ? move : -move;
                break;
        }
       if (transition > 1)      //We already reached next segment
        {
            transition = 0;     //Restart the count
            currentSeg++;       //Update the segment we are in
            if(currentSeg == rail.nodes.Length - 1) //Last segment reached
            {
                if (isLooping)
                {
                    if (pingPong)
                    {
                        transition = 1;
                        currentSeg = rail.nodes.Length - 2;
                        isReversed = !isReversed;
                    }
                    else
                    {
                        currentSeg = 0;
                    }
                }
            }
        }
        else if(transition < 0) //Same logic that before but backwards, we reached next segment
        {
            transition = 1;         //Restart the count
            currentSeg--;           //Update the segment we are in
            if (currentSeg == - 1)  //Last segment reached
            {
                if (isLooping)
                {
                    if (pingPong)
                    {
                        transition = 0;
                        currentSeg = 0;
                        isReversed = !isReversed;
                    }
                    else
                    {
                        currentSeg = rail.nodes.Length -2;
                    }
                }
            }
        }
        
        transform.position = rail.PositionOnRail(currentSeg, transition, playMode);
        transform.rotation = rail.OrientationOnRail(currentSeg, transition, orientationMode, transform, isReversed);

    }
}
