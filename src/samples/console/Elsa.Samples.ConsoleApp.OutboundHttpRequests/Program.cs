﻿using Elsa.Extensions;
using Elsa.Samples.ConsoleApp.OutboundHttpRequests.Workflows;
using Elsa.Workflows.Core.Contracts;
using Microsoft.Extensions.DependencyInjection;

// Setup service container.
var services = new ServiceCollection();

// Add Elsa services.
services.AddElsa(elsa => elsa.UseHttp());

// Build service container.
var serviceProvider = services.BuildServiceProvider();

// Resolve a workflow runner to run the workflow.
var workflowRunner = serviceProvider.GetRequiredService<IWorkflowRunner>();

// Run the workflow.
await workflowRunner.RunAsync<GetUsersWorkflow>();