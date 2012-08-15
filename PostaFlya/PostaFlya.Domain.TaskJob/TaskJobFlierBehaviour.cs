using System;
using System.Collections.Generic;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Location;
using WebSite.Infrastructure.Domain;

namespace PostaFlya.Domain.TaskJob
{
    [Serializable]
    public class TaskJobFlierBehaviour : FlierBehaviourBase<TaskJobFlierBehaviourInterface>, TaskJobFlierBehaviourInterface
    { 

        public Locations ExtraLocations { get; set; }
        public double MaxAmount { get; set; }

        #region FlierProperties
        public double CostOverhead
        {
            get { return _flierProperties.CostOverhead; }
            set { _flierProperties.CostOverhead = value; }
        }


        private FlierInterface _flier;
        public override FlierInterface Flier
        {
            get { return _flier; }
            set { 
                _flier = value;
                UpdateFlierProperties();
            }
        }

        private TaskJobFlierBehaviourFlierProperties _flierProperties = new TaskJobFlierBehaviourFlierProperties(new PropertyGroup());
        public override PropertyGroup FlierProperties
        {
            get { return _flierProperties != null ? _flierProperties.PropertyGroup : null; }
            set { 
                _flierProperties = value != null ? new TaskJobFlierBehaviourFlierProperties(value) : null;
                UpdateFlierProperties();
            }
        }
        #endregion

        private void UpdateFlierProperties()
        {
            if (Flier == null || _flierProperties == null) return;
            if (Flier.ExtendedProperties == null)
                Flier.ExtendedProperties = new PropertyGroupCollection();
            Flier.ExtendedProperties["taskjob"] = _flierProperties.PropertyGroup;
        }

    }

    [Serializable]
    public class TaskJobFlierBehaviourFlierProperties
    {
        private readonly PropertyGroup _source;

        public TaskJobFlierBehaviourFlierProperties(PropertyGroup source)
        {
            if (source == null)
                source = new PropertyGroup();
            _source = source;
            _source.Name = "taskjob";
        }

        public double CostOverhead
        {
            get { return (double)(_source["CostOverhead"] ?? 0.0); }
            set { _source["CostOverhead"] = value; }
        }

        public PropertyGroup PropertyGroup
        {
            get { return _source; }
        }

    }
}