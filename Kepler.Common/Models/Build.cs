﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Kepler.Common.Models.Common;

namespace Kepler.Common.Models
{
    public class Build : BuildObject
    {
        [DataMember]
        [DataType(DataType.DateTime)]
        public DateTime? StartDate { get; set; }

        [DataMember]
        [DataType(DataType.DateTime)]
        public DateTime? StopDate { get; set; }

        [DataMember]
        [DataType(DataType.Time)]
        public TimeSpan? Duration { get; set; }

        [DataMember]
        [DataType(DataType.Time)]
        public TimeSpan? PredictedDuration { get; set; }

        [DataMember]
        public long? BranchId { get; set; }

        [DataMember]
        public long TestCount { get; set; }

        [DataMember]
        public long TestsFailed { get; set; }
    }
}