﻿using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Kepler.Common.Models.Common;
using Kepler.Common.Repository;

namespace Kepler.Common.Models
{
    public class Project : InfoObject
    {
        public Dictionary<long, Branch> Branches { get; set; }

        [DataMember]
        public long? MainBranchId { get; set; }

        public Project()
        {
            Branches = new Dictionary<long, Branch>();
        }

        public void InitChildObjectsFromDb()
        {
            Branches = BranchRepository.Instance.Find(branch => branch.ProjectId == Id)
                .ToDictionary(item => item.Id, item => item);
        }
    }
}