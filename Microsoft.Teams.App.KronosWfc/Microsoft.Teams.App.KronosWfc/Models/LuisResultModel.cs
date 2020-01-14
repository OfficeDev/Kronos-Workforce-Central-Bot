﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Microsoft.Teams.App.KronosWfc.Models
{
    public class TopScoringIntent
    {
        public string intent { get; set; }
        public double score { get; set; }
    }
    public class Intent
    {
        public string intent { get; set; }
        public double score { get; set; }
    }
    public class Value
    {
        public string timex { get; set; }
        public string type { get; set; }
        public string value { get; set; }
        public string start { get; set; }
        public string end { get; set; }
    }
    public class Resolution
    {
        public List<Value> values { get; set; }
    }
    public class Entity
    {
        public string entity { get; set; }
        public string type { get; set; }
        public int startIndex { get; set; }
        public int endIndex { get; set; }
        public Resolution resolution { get; set; }
    }
    public class LuisResultModel
    {
        public string query { get; set; }
        public TopScoringIntent topScoringIntent { get; set; }
        public List<Intent> intents { get; set; }
        public List<Entity> entities { get; set; }
    }
    public class LuisEntity
    {
        public string entity { get; set; }
        public string type { get; set; }
        public int startIndex { get; set; }
        public int endIndex { get; set; }
        public LuisResolution resolution { get; set; }
    }

    public class LuisResolution
    {
        public LuisValue[] values { get; set; }
    }

    public class LuisValue
    {
        public string timex { get; set; }
        public string type { get; set; }
        public string value { get; set; }
        public string start { get; set; }
        public string end { get; set; }
    }
}