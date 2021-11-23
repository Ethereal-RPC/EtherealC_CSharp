﻿using System;

namespace EtherealC.Core.Manager.Event.Attribute
{
    [AttributeUsage(AttributeTargets.Method)]
    public class Event : System.Attribute
    {
        public Event(string Mapping)
        {
            this.Mapping = Mapping;
        }
        public string Mapping { get; set; }
    }
}