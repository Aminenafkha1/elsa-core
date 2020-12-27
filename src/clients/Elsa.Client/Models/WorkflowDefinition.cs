﻿using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Elsa.Client.Models
{
    [DataContract]
    public class WorkflowDefinition
    {
        public WorkflowDefinition()
        {
            Variables = new Variables();
            Activities = new List<ActivityDefinition>();
            Connections = new List<ConnectionDefinition>();
        }

        [DataMember(Order = 1)] public string Id { get; set; } = default!;
        [DataMember(Order = 2)] public string DefinitionVersionId { get; set; } = default!;
        [DataMember(Order = 3)] public string TenantId { get; set; } = default!;
        [DataMember(Order = 4)] public string? Name { get; set; }
        [DataMember(Order = 5)] public string? DisplayName { get; set; }
        [DataMember(Order = 6)] public string? Description { get; set; }
        [DataMember(Order = 7)] public int Version { get; set; }
        [DataMember(Order = 8)] public Variables? Variables { get; set; }
        [DataMember(Order = 9)] public WorkflowContextOptions? ContextOptions { get; set; }
        [DataMember(Order = 10)] public bool IsSingleton { get; set; }
        [DataMember(Order = 11)] public WorkflowPersistenceBehavior PersistenceBehavior { get; set; }
        [DataMember(Order = 12)] public bool DeleteCompletedInstances { get; set; }
        [DataMember(Order = 13)] public bool IsEnabled { get; set; }
        [DataMember(Order = 14)] public bool IsPublished { get; set; }
        [DataMember(Order = 15)] public bool IsLatest { get; set; }
        [DataMember(Order = 16)] public ICollection<ActivityDefinition> Activities { get; set; }
        [DataMember(Order = 17)] public ICollection<ConnectionDefinition> Connections { get; set; }
    }
}