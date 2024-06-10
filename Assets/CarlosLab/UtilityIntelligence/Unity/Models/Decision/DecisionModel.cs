#region

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using CarlosLab.Common;
using CarlosLab.Common.Extensions;
using UnityEngine;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    [DataContract]
    public class DecisionModel : ContainerItemModel<Decision>
    {
        #region Decision

        [DataMember(Name = nameof(Weight))]
        private float weight = 1.0f;
        public float Weight
        {
            get => weight;
            set
            {
                weight = value;
                Runtime.ScoreCalculator.Weight = value;
            }
        }
        
        [DataMember(Name = nameof(HasNoTarget))]
        private bool hasNoTarget;
        public bool HasNoTarget
        {
            get => hasNoTarget;
            set
            {
                hasNoTarget = value;
                Runtime.HasNoTarget = value;
            }
        }
        
        [DataMember(Name = nameof(KeepRunningUntilFinished))]
        private bool keepRunningUntilFinished;
        public bool KeepRunningUntilFinished
        {
            get => keepRunningUntilFinished;
            set
            {
                keepRunningUntilFinished = value;
                Runtime.KeepRunningUntilFinished = value;
            }
        }
        
        [DataMember(Name = nameof(MaxRepeatCount))]
        private int maxRepeatCount;
        public int MaxRepeatCount
        {
            get => maxRepeatCount;
            set
            {
                maxRepeatCount = value;
                Runtime.Task.MaxRepeatCount = value;
            }
        }

        public int CurrentRepeatCount
        {
            get
            {
                if(Runtime.Task != null) return Runtime.Task.CurrentRepeatCount;

                return 0;
            }
        }

        #endregion

        #region Target Filters
        
        public TargetFilterContainerModel TargetFilterContainer { get; internal set; }
        
        [DataMember(Name = nameof(TargetFilters))]
        private List<string> targetFilterNames = new();
        
        private List<TargetFilterModel> targetFilters;

        public IReadOnlyList<TargetFilterModel> TargetFilters
        {
            get
            {
                if (targetFilters == null)
                {
                    targetFilters = new ();
                    if (TargetFilterContainer != null)
                    {
                        foreach (string name in targetFilterNames)
                        {
                            if (TargetFilterContainer.TryGetItem(name, out TargetFilterModel targetFilter))
                                targetFilters.Add(targetFilter);
                        }
                    }
                }

                return targetFilters;
            }
        }
        
        public bool HasTargetFilter(string name)
        {
            return Runtime.HasTargetFilter(name);
        }

        public bool TryAddTargetFilter(int index, string name, TargetFilterModel targetFilter)
        {
            if (Runtime.TryAddTargetFilter(index, name, targetFilter.Runtime))
            {
                targetFilterNames.Insert(index, targetFilter.Name);
                targetFilters?.Insert(index, targetFilter);
                return true;
            }

            return false;
        }

        public TargetFilterModel GetTargetFilterById(string id)
        {
            return targetFilters.Find(model => model.Id == id);
        }

        public bool TryRemoveTargetFilter(string name, TargetFilterModel model)
        {
            if (Runtime.TryRemoveTargetFilter(name))
            {
                targetFilterNames.Remove(name);
                targetFilters?.Remove(model);
                return true;
            }

            return false;
        }

        public void OnTargetFilterNameChanged(string oldName, string newName)
        {
            int index = targetFilterNames.IndexOf(oldName);
            if(index >= 0) targetFilterNames[index] = newName;
        }
        
        public void MoveTargetFilter(int sourceIndex, int destIndex)
        {
            targetFilterNames.Move(sourceIndex, destIndex);
            targetFilters?.Move(sourceIndex, destIndex);
            Runtime.MoveTargetFilter(sourceIndex, destIndex);

        }

        #endregion

        #region Action Tasks

        [DataMember(Name = nameof(Actions))]
        private List<ActionModel> actions = new();

        public IReadOnlyList<ActionModel> Actions => actions;

        [DataMember(Name = nameof(ActionExecutionMode))]
        private ActionExecutionMode actionExecutionMode = ActionExecutionMode.Sequence;

        public ActionExecutionMode ActionExecutionMode => actionExecutionMode;

        public void SetActionsExecutionMode(ActionExecutionMode newMode, bool awakeTask)
        {
            if (actionExecutionMode == newMode) return;
            
            actionExecutionMode = newMode;
            var composite = CreateCompositeTask();
            composite.Intelligence = Runtime.Intelligence;
            Runtime.Task.Child = composite;
            if (awakeTask) composite.Awake();
        }
        
        public void AddAction(ActionModel action, bool awakeAction)
        {
            actions.Add(action);
            if (Runtime.Task.Child is Composite compositeTask)
            {
                compositeTask.AddChild(action.Runtime);
                if (awakeAction) action.Runtime.Awake();
            }
        }

        public void AddAction(int index, ActionModel action, bool awakeAction)
        {
            actions.Insert(index, action);
            if (Runtime.Task.Child is Composite compositeTask)
            {
                compositeTask.AddChild(index, action.Runtime);
                if (awakeAction) action.Runtime.Awake();
            }
        }

        public ActionModel GetActionById(string id)
        {
            return actions.Find(action => action.Id == id);
        }

        public void MoveAction(int sourceIndex, int destIndex)
        {
            actions.Move(sourceIndex, destIndex);
            if (Runtime.Task.Child is Composite compositeTask) compositeTask.MoveChild(sourceIndex, destIndex);
        }

        public void RemoveAction(ActionModel model)
        {
            actions.Remove(model);
            if (Runtime.Task.Child is Composite compositeTask) compositeTask.RemoveChild(model.Runtime);
        }

        #endregion

        #region Considerations
        public ConsiderationContainerModel ConsiderationContainer { get; internal set; }

        [DataMember(Name = nameof(Considerations))]
        private List<string> considerationNames = new();

        private List<ConsiderationModel> considerations;
        public IReadOnlyList<ConsiderationModel> Considerations
        {
            get
            {
                if (considerations == null)
                {
                    considerations = new List<ConsiderationModel>();

                    if (ConsiderationContainer != null)
                    {
                        foreach (string name in considerationNames)
                        {
                            if (ConsiderationContainer.TryGetItem(name, out ConsiderationModel consideration))
                                considerations.Add(consideration);
                        }
                    }
                }

                return considerations;
            }
        }

        public bool HasConsideration(string name)
        {
            return Runtime.ScoreCalculator.HasConsideration(name);
        }

        public bool TryAddConsideration(int index, string name, ConsiderationModel consideration)
        {
            if (Runtime.ScoreCalculator.TryAddConsideration(index, name, consideration.Runtime))
            {
                considerationNames.Insert(index, name);
                considerations?.Insert(index, consideration);
                return true;
            }

            return false;
        }

        public ConsiderationModel GetConsiderationById(string id)
        {
            return considerations.Find(consideration => consideration.Id == id);
        }

        public bool TryRemoveConsideration(string name, ConsiderationModel consideration)
        {
            if (Runtime.ScoreCalculator.TryRemoveConsideration(name))
            {
                considerationNames.Remove(name);
                considerations?.Remove(consideration);
                return true;
            }

            return false;
        }

        public void OnConsiderationNameChanged(string oldName, string newName)
        {
            int index = considerationNames.IndexOf(oldName);
            if(index >= 0) considerationNames[index] = newName;
        }

        public void MoveConsideration(int sourceIndex, int destIndex)
        {
            considerationNames.Move(sourceIndex, destIndex);
            considerations?.Move(sourceIndex, destIndex);
            Runtime.ScoreCalculator.MoveConsideration(sourceIndex, destIndex);
        }

        #endregion

        #region Runtime

        private Decision runtime;

        private AbortTaskCommand AbortTaskCommand { get; } = new();


        public override Decision Runtime
        {
            get
            {
                if (runtime == null)
                {
                    Repeater task = CreateRepeaterTask();
                    DecisionScoreCalculator scoreCalculator = CreateScoreCalculator();
                    runtime = new Decision(scoreCalculator, task)
                    {
                        KeepRunningUntilFinished = keepRunningUntilFinished,
                        HasNoTarget = hasNoTarget
                    };
                    for (int index = 0; index < TargetFilters.Count; index++)
                    {
                        TargetFilterModel targetFilter = TargetFilters[index];
                        runtime.TryAddTargetFilter(targetFilter.Name, targetFilter.Runtime);
                    }
                }

                return runtime;
            }
        }
        
        private DecisionScoreCalculator CreateScoreCalculator()
        {
            DecisionScoreCalculator scoreEvaluator = new(Weight);
            for (int index = 0; index < Considerations.Count; index++)
            {
                ConsiderationModel consideration = Considerations[index];
                scoreEvaluator.TryAddConsideration(consideration.Name, consideration.Runtime);
            }

            return scoreEvaluator;
        }

        private Composite CreateCompositeTask()
        {
            Composite composite = null;
            switch (actionExecutionMode)
            {
                case ActionExecutionMode.Sequence:
                    composite = new Sequencer();
                    break;
                case ActionExecutionMode.Parallel:
                    composite = new Parallel();
                    break;
                case ActionExecutionMode.ParallelComplete:
                    composite = new ParallelComplete();
                    break;
            }

            foreach (ActionModel action in actions)
            {
                composite?.AddChild(action.Runtime);
            }

            return composite;
        }

        private Repeater CreateRepeaterTask()
        {
            Task childTask = CreateCompositeTask();
            
            Repeater repeater = new()
            {
                Child = childTask,
                MaxRepeatCount = this.maxRepeatCount,
                BeforeChangeContextCommand = AbortTaskCommand
            };

            return repeater;
        }

        #endregion
        
        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            if (context.Context is UtilityIntelligenceAsset asset)
            {
                asset.Decisions.Add(this);
            }
        }
    }
}