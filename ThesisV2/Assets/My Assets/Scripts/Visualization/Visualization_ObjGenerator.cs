﻿using UnityEngine;
using System;
using System.Collections.Generic;
using Thesis.Interface;
using Thesis.Utility;

namespace Thesis.Visualization
{
    public class Visualization_ObjGenerator : MonoBehaviour
    {
        //--- Methods ---//
        public static List<Visualization_Object> GenerateVisObjects(List<Visualization_ObjParse> _parsedObjects, string _parentName)
        {
            // Create a list to hold all of the generated objects
            List<Visualization_Object> generatedObjects = new List<Visualization_Object>();

            // Generate a new gameobject to be the parent of all the spawned objects
            GameObject parentObj = new GameObject("--- " + _parentName + " ---");
            Transform parentTransform = parentObj.transform;

            // Loop through and generate actual visualization objects from the parsed ones
            foreach (Visualization_ObjParse objParse in _parsedObjects)
            {
                // Create a new gameobject with the name from the parser and make it a child of the parent transform
                GameObject visObj = new GameObject(objParse.m_objName);
                visObj.transform.parent = parentTransform;

                // Give the object the visualization script
                Visualization_Object visObjComp = visObj.AddComponent<Visualization_Object>();
                visObjComp.Setup();

                // Attach the related tracks to the object and connect them to the visualization script
                foreach(KeyValuePair<string, string> trackInfo in objParse.m_trackData)
                {
                    // Add the track by name and get the interface reference from it
                    string trackNamespace = "Thesis.VisTrack";
                    string trackName = "VisTrack_" + trackInfo.Key;
                    string fullTrackName = trackNamespace + "." + trackName;
                    Type trackType = Utility_Functions.GetTypeFromString(fullTrackName);
                    IVisualizable trackComp = visObj.AddComponent(trackType) as IVisualizable;

                    // If it failed to add properly, return false
                    if (trackComp == null)
                        return null;

                    // Initialize the track with the data that was parsed. Return false if it failed
                    if (!trackComp.InitWithString(trackInfo.Value))
                        return null;

                    // Register the track with the visualization object
                    visObjComp.AddTrack(trackComp);

                    // Add the object to the list
                    generatedObjects.Add(visObjComp);
                }
            }

            // If all the objects were generated correctly, return them
            return generatedObjects;
        }
    }

}