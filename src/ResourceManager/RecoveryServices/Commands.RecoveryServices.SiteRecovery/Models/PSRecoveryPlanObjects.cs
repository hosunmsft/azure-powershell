// ----------------------------------------------------------------------------------
// 
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Azure.Management.RecoveryServices.SiteRecovery.Models;

namespace Microsoft.Azure.Commands.RecoveryServices.SiteRecovery
{
    public class ASRRecoveryPlanGroup
    {
        public ASRRecoveryPlanGroup()
        {
        }

        public ASRRecoveryPlanGroup(
            RecoveryPlanGroup recoveryPlanGroup,
            IList<ReplicationProtectedItem> replicationProtectedItems = null)
        {
            if (recoveryPlanGroup != null)
            {
                this.GroupType = recoveryPlanGroup.GroupType.ToString(); //TODO
                this.StartGroupActions = recoveryPlanGroup.StartGroupActions;
                this.EndGroupActions = recoveryPlanGroup.EndGroupActions;

                if (replicationProtectedItems != null)
                {
                    var replicationProtectedItemList =
                        recoveryPlanGroup.ReplicationProtectedItems.Select(
                            item => item.Id.ToLower());
                    this.ReplicationProtectedItems = replicationProtectedItems.Where(
                            rpi => replicationProtectedItemList.Contains(rpi.Id.ToLower()))
                        .ToList();
                }
                else
                {
                    this.ReplicationProtectedItems = new List<ReplicationProtectedItem>();
                }
            }
        }

        public ASRRecoveryPlanGroup(
            string groupName,
            RecoveryPlanGroup recoveryPlanGroup,
            IList<ReplicationProtectedItem> replicationProtectedItems = null) : this(
            recoveryPlanGroup,
            replicationProtectedItems)
        {
            this.Name = groupName;
        }

        /// <summary>
        ///     Gets or sets Recovery plan group Name.
        /// </summary>
        public string Name { get; set; }

        // Summary:
        //     Optional. Recovery plan end group actions.
        public IList<RecoveryPlanAction> EndGroupActions { get; set; }

        //
        // Summary:
        //     Required. Group type.
        public string GroupType { get; set; }

        //
        // Summary:
        //     Optional. List of protected items.
        public IList<ReplicationProtectedItem> ReplicationProtectedItems { get; set; }

        //
        // Summary:
        //     Optional. Recovery plan start group actions.
        public IList<RecoveryPlanAction> StartGroupActions { get; set; }
    }

    /// <summary>
    ///     Azure Site Recovery Recovery Plan.
    /// </summary>
    [SuppressMessage(
        "Microsoft.StyleCop.CSharp.MaintainabilityRules",
        "SA1402:FileMayOnlyContainASingleClass",
        Justification = "Keeping all related objects together.")]
    public class ASRRecoveryPlan
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ASRRecoveryPlan" /> class.
        /// </summary>
        public ASRRecoveryPlan()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ASRRecoveryPlan" /> class with required
        ///     parameters.
        /// </summary>
        /// <param name="recoveryPlan">Recovery plan object</param>
        public ASRRecoveryPlan(
            RecoveryPlan recoveryPlan,
            IList<ReplicationProtectedItem> replicationProtectedItems)
        {
            this.Id = recoveryPlan.Id;
            this.Name = recoveryPlan.Name;
            this.FriendlyName = recoveryPlan.Properties.FriendlyName;
            this.ServerId = recoveryPlan.Properties.PrimaryFabricId;
            this.TargetServerId = recoveryPlan.Properties.RecoveryFabricId;
            this.FailoverDeploymentModel = recoveryPlan.Properties.FailoverDeploymentModel;
            this.Groups = new List<ASRRecoveryPlanGroup>();
            var groupCount = 0;
            string groupName = null;

            foreach (var recoveryPlanGroup in recoveryPlan.Properties.Groups)
            {
                switch (recoveryPlanGroup.GroupType)
                {
                    case RecoveryPlanGroupType.Boot:
                        groupCount++;
                        groupName = "Group " + groupCount;
                        break;
                    case RecoveryPlanGroupType.Failover:
                        groupName = Constants.Failover;
                        break;
                    case RecoveryPlanGroupType.Shutdown:
                        groupName = Constants.Shutdown;
                        break;
                }

                this.Groups.Add(
                    new ASRRecoveryPlanGroup(
                        groupName,
                        recoveryPlanGroup,
                        replicationProtectedItems));
            }

            this.ReplicationProvider = recoveryPlan.Properties.ReplicationProviders;
        }

        /// <summary>
        ///     Refreshes group names for the RP
        /// </summary>
        /// <returns></returns>
        public ASRRecoveryPlan RefreshASRRecoveryPlanGroupNames()
        {
            var bootGroupCount = 0;
            for (var groupCount = 0; groupCount < this.Groups.Count; groupCount++)
            {
                switch (this.Groups[groupCount]
                    .GroupType)
                {
                    case Constants.Boot:
                        bootGroupCount++;
                        this.Groups[groupCount]
                            .Name = "Group " + bootGroupCount;
                        break;
                    case Constants.Failover:
                        this.Groups[groupCount]
                            .Name = Constants.Failover;
                        break;
                    case Constants.Shutdown:
                        this.Groups[groupCount]
                            .Name = Constants.Shutdown;
                        break;
                }
            }

            return this;
        }

        #region Properties

        /// <summary>
        ///     Gets or sets Recovery plan Name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets FriendlyName of the Recovery Plan.
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        ///     Gets or sets to Server ID.
        /// </summary>
        public string ServerId { get; set; }

        /// <summary>
        ///     Gets or sets target Server ID.
        /// </summary>
        public string TargetServerId { get; set; }

        /// <summary>
        ///     Gets or sets Failover Deployment Model
        /// </summary>
        public string FailoverDeploymentModel { get; set; }

        /// <summary>
        ///     Gets or sets ASRGroups
        /// </summary>
        public IList<ASRRecoveryPlanGroup> Groups { get; set; }

        /// <summary>
        ///     Gets or sets Replication provider.
        /// </summary>
        public IList<string> ReplicationProvider { get; set; }

        /// <summary>
        ///     Gets or sets ID.
        /// </summary>
        public string Id { get; set; }

        #endregion
    }
}