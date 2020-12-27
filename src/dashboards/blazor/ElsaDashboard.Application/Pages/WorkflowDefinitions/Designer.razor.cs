﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Elsa.Client.Models;
using ElsaDashboard.Application.Models;
using ElsaDashboard.Application.Services;
using ElsaDashboard.Shared.Rpc;
using Microsoft.AspNetCore.Components;

namespace ElsaDashboard.Application.Pages.WorkflowDefinitions
{
    partial class Designer
    {
        [Parameter] public string? WorkflowDefinitionVersionId { get; set; }
        [Inject] private IWorkflowDefinitionService WorkflowDefinitionService { get; set; } = default!;
        [Inject] private IActivityService ActivityService { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        private IDictionary<string, ActivityInfo> ActivityDescriptors { get; set; } = default!;

        private WorkflowDefinition WorkflowDefinition { get; set; } = new()
        {
            Name = "Untitled",
            DisplayName = "Untitled",
            Version = 1
        };

        private WorkflowModel WorkflowModel { get; set; } = WorkflowModel.Blank();
        private BackgroundWorker BackgroundWorker { get; } = new();
        private string DisplayName => !string.IsNullOrWhiteSpace(WorkflowDefinition.DisplayName) ? WorkflowDefinition.DisplayName : !string.IsNullOrWhiteSpace(WorkflowDefinition.Name) ? WorkflowDefinition.Name : "Untitled";
        private static TabItem[] Tabs => new[] {new TabItem("Designer"), new TabItem("DSL", "Dsl"), new TabItem("Settings")};
        private TabItem CurrentTab { get; set; } = Tabs.First();

        protected override async Task OnInitializedAsync()
        {
            ActivityDescriptors = (await ActivityService.GetActivitiesAsync()).ToDictionary(x => x.Type);
            StartBackgroundWorker();
        }

        protected override async Task OnParametersSetAsync()
        {
            if (WorkflowDefinitionVersionId != null)
            {
                WorkflowDefinition = await WorkflowDefinitionService.GetByVersionIdAsync(WorkflowDefinitionVersionId);
                WorkflowModel = CreateWorkflowModel(WorkflowDefinition);
            }
            else
            {
                WorkflowDefinition = new WorkflowDefinition();
                WorkflowModel = WorkflowModel.Blank();
            }
        }

        private async ValueTask SaveWorkflowAsync()
        {
            var request = new SaveWorkflowDefinitionRequest
            {
                WorkflowDefinitionId = WorkflowDefinition.Id,
                Activities = WorkflowModel.Activities.Select(CreateActivityDefinition).ToList(),
                Connections = WorkflowModel.Connections.Select(x => new ConnectionDefinition(x.SourceId, x.TargetId, x.Outcome)).ToList(),
                Name = WorkflowDefinition.Name,
                DisplayName = WorkflowDefinition.DisplayName,
                Description = WorkflowDefinition.Description,
                Enabled = WorkflowDefinition.IsEnabled,
                Publish = false,
                Variables = WorkflowDefinition.Variables,
                ContextOptions = WorkflowDefinition.ContextOptions,
                IsSingleton = WorkflowDefinition.IsSingleton,
                PersistenceBehavior = WorkflowDefinition.PersistenceBehavior,
                DeleteCompletedInstances = WorkflowDefinition.DeleteCompletedInstances
            };

            var isNew = WorkflowDefinition.Id == null!;
            var savedWorkflowDefinition = await WorkflowDefinitionService.SaveAsync(request);

            if (isNew)
                NavigationManager.NavigateTo($"workflow-definitions/{savedWorkflowDefinition.DefinitionVersionId}/designer");
        }

        private ActivityDefinition CreateActivityDefinition(ActivityModel activityModel)
        {
            return new()
            {
                ActivityId = activityModel.ActivityId,
                Type = activityModel.Type
            };
        }

        private WorkflowModel CreateWorkflowModel(WorkflowDefinition workflowDefinition)
        {
            return new()
            {
                Name = workflowDefinition.Name,
                Activities = workflowDefinition.Activities.Select(CreateActivityModel).ToImmutableList(),
                Connections = workflowDefinition.Connections.Select(CreateConnectionModel).ToImmutableList()
            };
        }

        private ConnectionModel CreateConnectionModel(ConnectionDefinition connectionDefinition) => new(connectionDefinition.SourceActivityId, connectionDefinition.TargetActivityId, connectionDefinition.Outcome);

        private ActivityModel CreateActivityModel(ActivityDefinition activityDefinition)
        {
            var descriptor = ActivityDescriptors[activityDefinition.Type];
            return new ActivityModel
            {
                Name = activityDefinition.Name,
                ActivityId = activityDefinition.ActivityId,
                Type = activityDefinition.Type,
                DisplayName = descriptor.DisplayName,
                Outcomes = descriptor.Outcomes
            };
        }

        private void SelectTab(TabItem tab)
        {
            CurrentTab = tab;
        }

        private void StartBackgroundWorker()
        {
#pragma warning disable 4014
            InvokeAsync(() => BackgroundWorker.StartAsync());
#pragma warning restore 4014
        }

        private async Task OnWorkflowChanged(WorkflowModelChangedEventArgs e)
        {
            WorkflowModel = e.WorkflowModel;
            await BackgroundWorker.ScheduleTask(SaveWorkflowAsync);
        }

        private async Task OnSettingsChanged()
        {
            await BackgroundWorker.ScheduleTask(SaveWorkflowAsync); 
        }
    }
}