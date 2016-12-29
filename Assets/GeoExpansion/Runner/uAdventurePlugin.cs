﻿using UnityEngine;
using System.Collections;
using MapzenGo.Models.Plugins;
using MapzenGo.Models;
using System;
using System.Collections.Generic;

using uAdventure.Runner;

namespace uAdventure.Geo
{
    public class uAdventurePlugin : Plugin
    {
        MapSceneMB mapSceneMB;

        public List<MapElement> OrphanElements
        {
            get; private set;
        }

        public List<MapElement> AdoptedElements
        {
            get; private set;
        }

        void Awake()
        {
            mapSceneMB = FindObjectOfType<MapSceneMB>();
            OrphanElements = new List<MapElement>();
            AdoptedElements = new List<MapElement>();
        }

        protected override IEnumerator CreateRoutine(Tile tile, Action<bool> finished)
        {
            
            var allElements = mapSceneMB.MapElements.FindAll(elem => elem.Conditions == null || ConditionChecker.check(elem.Conditions));
            foreach(var elem in allElements)
            {
                if (!AdoptedElements.Contains(elem) && !OrphanElements.Contains(elem))
                {
                    OrphanElements.Add(elem);
                }
            }

            finished(true);

            yield return null;
        }

        public bool AdoptElement(MapElement mapElement)
        {
            if(OrphanElements.Contains(mapElement))
            {
                AdoptedElements.Add(mapElement);
                OrphanElements.Remove(mapElement);
                return true;
            }

            return false;
        }

        public void ReleaseElement(MapElement mapElement)
        {
            if (AdoptedElements.Contains(mapElement))
            {
                AdoptedElements.Remove(mapElement);
                OrphanElements.Add(mapElement);
            }
        }
    }
}
