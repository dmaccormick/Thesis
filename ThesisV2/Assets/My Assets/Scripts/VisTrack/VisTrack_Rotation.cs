﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Thesis.Interface;
using Thesis.Utility;


namespace Thesis.VisTrack
{
    public class VisTrack_Rotation : MonoBehaviour, IVisualizable
    {
        //--- Data Struct ---//
        [System.Serializable]
        public struct Data_Rotation
        {
            public Data_Rotation(string _dataStr)
            {
                // Split the data string
                string[] tokens = _dataStr.Split('~');

                // The first token is the timestamp so just parse the float
                m_timestamp = float.Parse(tokens[0]);

                // The second token is the quaternion so we need to parse that specifically
                this.m_data = Utility_Functions.ParseQuaternion(tokens[1]);
            }

            public static List<Data_Rotation> ParseDataList(string _data)
            {
                // Create a list to hold the parsed data
                List<Data_Rotation> dataPoints = new List<Data_Rotation>();

                // Split the string into individual lines which each are one data point
                string[] lines = _data.Split('\n');

                // Create new data points from each of the lines
                foreach (string line in lines)
                {
                    // If the line is empty, do nothing
                    if (line == null || line == "")
                        continue;

                    // Otherwise, create a new data point
                    dataPoints.Add(new Data_Rotation(line));
                }

                // Return the list of data points
                return dataPoints;
            }

            public float m_timestamp;
            public Quaternion m_data;
        }



        //--- Private Variables ---//
        private List<Data_Rotation> m_dataPoints;



        //--- IVisualizable Interface ---// 
        public bool InitWithString(string _data)
        {
            try
            {
                // Create a list of data points by parsing the string
                m_dataPoints = Data_Rotation.ParseDataList(_data);

                // If everything worked correctly, return true
                return true;
            }
            catch (Exception _e)
            {
                // If something went wrong, output an error and return false
                Debug.LogError("Error in InitWithString(): " + _e.Message);
                return false;
            }
        }

        public void StartVisualization(float _startTime)
        {
        }

        public void UpdateVisualization(float _time)
        {
            throw new System.NotImplementedException();
        }

        public int FindDataPointForTime(float _time)
        {
            throw new System.NotImplementedException();
        }

        public string GetTrackName()
        {
            return "Rotation";
        }

        public float GetFirstTimestamp()
        {
            // Ensure the datapoints are actually setup
            Assert.IsNotNull(m_dataPoints, "m_dataPoints has to be setup for before looking for a data point");
            Assert.IsTrue(m_dataPoints.Count >= 1, "m_dataPoints cannot be empty");

            // Return the timestamp for the first data point
            return m_dataPoints[0].m_timestamp;
        }
    }

}