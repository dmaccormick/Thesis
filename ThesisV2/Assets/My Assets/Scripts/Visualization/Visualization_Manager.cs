﻿using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Assertions;
using Thesis.FileIO;
using System.Collections.Generic;

namespace Thesis.Visualization
{
    //--- Playstate Enum ---//
    public enum Playstate
    {
        Paused,
        Reverse,
        Forward,
    }



    //--- Event Classes ---//
    [System.Serializable] public class FloatEvent : UnityEvent<float>
    {
    }

    [System.Serializable] public class PlaystateEvent : UnityEvent<Playstate>
    {
    }



    //--- Visualization Manager ---//
    // NOTE: The visualization manager must be placed first in the script execution order
    [DefaultExecutionOrder(-10)]
    public class Visualization_Manager : MonoBehaviour
    {
        //--- Public Variables ---//
        public UnityEvent<float> m_onTimeUpdated;
        public UnityEvent<Playstate> m_onPlaystateUpdated;


             
        //--- Private Variables ---//
        private List<Visualization_Object> m_staticObjects; // TODO: Create object set class that allows for hiding / activating of the whole set
        private List<List<Visualization_Object>> m_dynamicObjects;
        private Playstate m_playstate;
        private float m_startTime = Mathf.Infinity;
        private float m_endTime = 0.0f;
        private float m_currentTime = 0.0f;
        private float m_playbackSpeed = 1.0f;



        //--- Unity Methods ---//
        private void Awake()
        {
            // Init the events
            m_onTimeUpdated = new FloatEvent();
            m_onPlaystateUpdated = new PlaystateEvent();
        }

        private void Update()
        {
            // If in playmode, we should update the timer
            if (m_playstate == Playstate.Forward)
            {
                // Move the time forward
                m_currentTime += (Time.deltaTime * m_playbackSpeed);

                // If the time has reached the end, we should pause the playback and cap the timer
                if (m_currentTime >= m_endTime)
                {
                    // Cap the time
                    m_currentTime = m_endTime;

                    // Pause the playback
                    m_playstate = Playstate.Paused;

                    // Trigger the event associated with updating the playback state
                    m_onPlaystateUpdated.Invoke(m_playstate);
                }

                // Trigger the time update event
                m_onTimeUpdated.Invoke(m_currentTime);

                // Update the visualization to the current point in time
                UpdateVisualization();
            }
            else if (m_playstate == Playstate.Reverse)
            {
                // Move the time backwards
                m_currentTime -= (Time.deltaTime * m_playbackSpeed);

                // If the time has reached the start, we should pause the playback and cap the timer
                if (m_currentTime <= m_startTime)
                {
                    // Cap the time
                    m_currentTime = m_startTime;

                    // Pause the playback
                    m_playstate = Playstate.Paused;

                    // Trigger the event associated with updating the playback state
                    m_onPlaystateUpdated.Invoke(m_playstate);
                }

                // Trigger the time update event
                m_onTimeUpdated.Invoke(m_currentTime);

                // Update the visualization to the current point in time
                UpdateVisualization();
            }
        }



        //--- Loading Methods ---//
        public bool LoadStaticData(string _staticFilePath)
        {
            // We should pause the playback now and therefore trigger the event
            m_playstate = Playstate.Paused;
            m_onPlaystateUpdated.Invoke(m_playstate);

            // Read all of the data from the static file
            string staticData = FileIO_FileReader.ReadFile(_staticFilePath);

            // If the file didn't read correctly, return false
            if (staticData == null)
                return false;

            // Send the data to the parser and get the list of objects back
            List<Visualization_ObjParse> parsedStaticObjects = Visualization_LogParser.ParseLogFile(staticData);

            // If the parse failed, return false
            if (parsedStaticObjects == null)
                return false;

            // Generate actual objects from the list of parsed objects
            m_staticObjects = Visualization_ObjGenerator.GenerateVisObjects(parsedStaticObjects, "Static Objects");

            // Return false if the object generation failed
            if (m_staticObjects == null)
                return false;

            // Look for the new start and end times and then invoke the relevant event
            m_startTime = CalcNewStartTime();
            m_endTime = CalcNewEndTime();
            m_currentTime = m_startTime;
            m_onTimeUpdated.Invoke(m_currentTime);

            // Loop through all of the static objects and start their visualizations
            foreach (Visualization_Object visObj in m_staticObjects)
                visObj.StartVisualization(m_startTime);

            // Return true if everything parsed correctly
            return true;
        }

        public bool LoadDynamicData(string _dynamicFilePath)
        {
            // We should pause the playback now and therefore trigger the event
            m_playstate = Playstate.Paused;
            m_onPlaystateUpdated.Invoke(m_playstate);

            // If this is the first dynamic object list added, need to setup the outer list
            if (m_dynamicObjects == null)
                m_dynamicObjects = new List<List<Visualization_Object>>();

            // Read all of the data from the dynamic file
            string dynamicData = FileIO_FileReader.ReadFile(_dynamicFilePath);

            // If the file didn't read correctly, return false
            if (dynamicData == null)
                return false;

            // Send the data to the parser and get the list of objects back
            List<Visualization_ObjParse> parsedDynamicObjects = Visualization_LogParser.ParseLogFile(dynamicData);

            // If the parse failed, return false
            if (parsedDynamicObjects == null)
                return false;

            // Generate actual objects from the list of parsed objects
            m_dynamicObjects.Add(Visualization_ObjGenerator.GenerateVisObjects(parsedDynamicObjects, "Dynamic Objects"));

            // Return false if the object generation failed
            if (m_dynamicObjects == null)
                return false;

            // Look for the new start and end times and then invoke the relevant event
            m_startTime = CalcNewStartTime();
            m_endTime = CalcNewEndTime();
            m_currentTime = m_startTime;
            m_onTimeUpdated.Invoke(m_currentTime);

            // Loop through all of the newly added dynamic objects and start their visualizations
            foreach (Visualization_Object visObj in m_dynamicObjects[m_dynamicObjects.Count - 1])
                visObj.StartVisualization(m_startTime);

            // Return true if everything parsed correctly
            return true;
        }



        //--- Playback Methods ---//
        public void ReversePlayback()
        {
            // Update the playstate
            m_playstate = Playstate.Reverse;
        }

        public void PausePlayback()
        {
            // Update the playstate
            m_playstate = Playstate.Paused;
        }

        public void PlayForward()
        {
            // Update the playstate
            m_playstate = Playstate.Forward;
        }

        public void UpdateVisualization()
        {
            // Update dynamic objects, don't update the static ones
            if (m_dynamicObjects != null)
            {
                // Loop through all of the dynamic visualization objects and update their visualizations
                foreach(List<Visualization_Object> dynamicObjSet in m_dynamicObjects)
                {
                    foreach(Visualization_Object visObj in dynamicObjSet)
                    {
                        visObj.UpdateVisualization(m_currentTime);
                    }
                }
            }
        }



        //--- Setters ---//
        public void SetCurrentTime(float _time)
        {
            // Ensure the given time is in the range of the start and end values
            Assert.IsTrue(_time <= m_endTime, "The new time cannot be larger than the end time");
            Assert.IsTrue(_time >= m_startTime, "The new time cannot be smaller than the start time");

            // Update the current time
            m_currentTime = _time;

            // Update the visualization
            UpdateVisualization();
        }

        public void SetPlaybackSpeed(float _newSpeed)
        {
            // Update the playback speed
            m_playbackSpeed = _newSpeed;
        }



        //--- Getters ---//
        public float GetStartTime()
        {
            return m_startTime;
        }

        public float GetEndTime()
        {
            return m_endTime;
        }

        public float GetCurrentTime()
        {
            return m_currentTime;
        }



        //--- Utility Functions ---//
        private float CalcNewStartTime()
        {
            // Set the start time to a very high number to start
            float startTime = Mathf.Infinity;

            // If the static objects are setup, see which of them has the earliest start time
            if (m_staticObjects != null)
                startTime = Mathf.Min(startTime, GetEarliestTimeFromVisObjSet(m_staticObjects));

            // Do the same for each of the dynamic object lists if they are setup
            if (m_dynamicObjects != null)
            {
                foreach(List<Visualization_Object> dynamicObjectSet in m_dynamicObjects)
                    startTime = Mathf.Min(startTime, GetEarliestTimeFromVisObjSet(dynamicObjectSet));
            }

            // Return the earliest time
            return startTime;
        }

        private float CalcNewEndTime()
        {
            // Set the end time to a very low number to start
            float endTime = 0.0f;

            // If the static objects are setup, see which of them has the latest end time
            if (m_staticObjects != null)
                endTime = Mathf.Max(endTime, GetLatestTimeFromVisObjSet(m_staticObjects));

            // Do the same for each of the dynamic object lists if they are setup
            if (m_dynamicObjects != null)
            {
                foreach (List<Visualization_Object> dynamicObjectSet in m_dynamicObjects)
                    endTime = Mathf.Max(endTime, GetLatestTimeFromVisObjSet(dynamicObjectSet));
            }

            // Return the latest time
            return endTime;
        }

        private float GetEarliestTimeFromVisObjSet(List<Visualization_Object> _objectSet)
        {
            // Set the start time to a very high number to start
            float startTime = Mathf.Infinity;

            // Loop through all of the objects and find which of them has the latest end time
            foreach (Visualization_Object visObj in _objectSet)
                startTime = Mathf.Min(startTime, visObj.GetEarliestTrackTime());

            // Return the earliest time
            return startTime;
        }

        private float GetLatestTimeFromVisObjSet(List<Visualization_Object> _objectSet)
        {
            // Set the end time to a very low number to start
            float endTime = 0.0f;

            // Loop through all of the objects and find which of them has the latest end time
            foreach (Visualization_Object visObj in _objectSet)
                endTime = Mathf.Max(endTime, visObj.GetLatestTrackTime());

            // Return the latest time
            return endTime;
        }
    }
}